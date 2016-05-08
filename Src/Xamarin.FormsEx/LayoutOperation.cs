using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Xamarin.FormsEx
{
    internal class LayoutOperation
    {
        static readonly BindablePropertyKey LayoutOperationsPropertyKey =
            BindableProperty.CreateAttachedReadOnly(
                "LayoutOperations",
                typeof(List<LayoutOperation>),
                typeof(LayoutOperation),
                null);

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="element">The element that caused the layout operation.</param>
        /// <param name="other">The list of other elements that were affected by the operation.</param>
        /// <param name="direction">The direction that the layout should be performed in.</param>
        /// <param name="value">The value that the elements will be affected by in the layout direction.</param>
        internal LayoutOperation(VisualElement element, IEnumerable<VisualElement> other, LayoutDirection direction, double value)
        {
            RootElement = element;
            Elements = new [] { element }.Union(other).ToList();
            Direction = direction;
            Value = value;
        }

        /// <summary>
        /// Gets the relative layout that is to be used as the container.
        /// </summary>
        /// <param name="element">The element to get the parent layout for.</param>
        /// <returns>The relative layout that is the parent of the given element.</returns>
        internal static RelativeLayout GetContainer(Element element)
        {
            if (element == null)
            {
                throw new InvalidOperationException("Can not find the parent layout for the given element.");
            }

            if (element.Parent is RelativeLayout)
            {
                return (RelativeLayout)element.Parent;
            }

            return GetContainer(element.Parent);
        }

        /// <summary>
        /// Returns the LayoutOperations that is currently applied to the bindable object.
        /// </summary>
        /// <param name="container">The instance to return the layout operation for.</param>
        /// <returns>The layout operation that is assigned to the instance.</returns>
        static List<LayoutOperation> GetLayoutOperations(RelativeLayout container)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            var operations = container.GetValue(LayoutOperationsPropertyKey.BindableProperty) as List<LayoutOperation>;

            if (operations == null)
            {
                operations = new List<LayoutOperation>();

                container.SetValue(LayoutOperationsPropertyKey, operations);
            }

            return operations;
        }

        /// <summary>
        /// Push an operation onto the stack.
        /// </summary>
        /// <param name="operation">The operation to push onto the stack.</param>
        /// <returns>The layout operation that was pushed onto the stack.</returns>
        internal static LayoutOperation Push(LayoutOperation operation)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            GetLayoutOperations(GetContainer(operation.RootElement)).Add(operation);

            return operation;
        }

        /// <summary>
        /// Push an operation onto the stack.
        /// </summary>
        /// <param name="element">The element to pop an operation for.</param>
        internal static LayoutOperation Pop(VisualElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            var operations = GetLayoutOperations(GetContainer(element));

            for (var i = operations.Count - 1; i >= 0; i--)
            {
                if (operations[i].RootElement == element)
                {
                    var operation = operations[i];

                    operations.RemoveAt(i);

                    return operation;
                }
            }

            return null;
        }

        /// <summary>
        /// Calculate the translation point for the current element.
        /// </summary>
        /// <param name="element">The element to calculate the translation point for.</param>
        /// <returns>The point at which to translate the element to.</returns>
        static Point CalculateTranslationPoint(VisualElement element)
        {
            var translationX = 0.0;
            var translationY = 0.0;

            foreach (var operation in GetLayoutOperations(GetContainer(element)).Where(op => op.Elements.Contains(element)))
            {
                switch (operation.Direction)
                {
                    case LayoutDirection.Horizontal:
                        translationX += operation.Value;
                        break;

                    case LayoutDirection.Vertical:
                        translationY += operation.Value;
                        break;
                }
            }

            return new Point(translationX, translationY);
        }

        /// <summary>
        /// Returns a value indicating whether theroot element can be translated with the current operation.
        /// </summary>
        /// <param name="element">The element to calculate the translation point for.</param>
        /// <returns>true if the root can be translated with the operation, false if not.</returns>
        static bool CanTranslate(VisualElement element)
        {
            var point = CalculateTranslationPoint(element);

            return Math.Abs(element.TranslationX - point.X) > Double.Epsilon || Math.Abs(element.TranslationY - point.Y) > Double.Epsilon;
        }

        /// <summary>
        /// Perform the translation for the element according to the operations that have been applied.
        /// </summary>
        /// <param name="element">The element to translate.</param>
        /// <param name="length">The speed of the translation.</param>
        /// <returns>A task which asynchronously performs the operation.</returns>
        internal static Task TranslateAsync(VisualElement element, uint length)
        {
            if (CanTranslate(element) == false)
            {
                return Task.FromResult(0);
            }

            var point = CalculateTranslationPoint(element);

            return TranslateToAsync(element, point.X, point.Y, length);
        }

        /// <summary>
        /// Perform the translation for the element according to the operations that have been applied.
        /// </summary>
        /// <param name="elements">The list of elements to perform a translation to according to the state defined in the stack.</param>
        /// <param name="length">The speed of the translation.</param>
        /// <returns>A task which asynchronously performs the operation.</returns>
        internal static Task TranslateAsync(IEnumerable<VisualElement> elements, uint length)
        {
            return Task.WhenAll(elements.Select(element => TranslateAsync(element, length)));
        }

        /// <summary>
        /// Perform a flyout on a horizontal basis.
        /// </summary>
        /// <param name="element">The element to perform the flyout for.</param>
        /// <param name="translationX">The translation amount to animate on the X scale.</param>
        /// <param name="translationY">The translation amount to animate on the Y scale.</param>
        /// <param name="length">The speed in which to perform the animation for the flyout.</param>
        /// <returns>A task which asynchronously performs the operation.</returns>
        static Task TranslateToAsync(VisualElement element, double translationX, double translationY, uint length)
        {
            if (Math.Abs(element.TranslationX - translationX) < Double.Epsilon && Math.Abs(element.TranslationY - translationY) < Double.Epsilon)
            {
                return Task.FromResult(0);
            }

            if (length > 0)
            {
                return element.TranslateTo(translationX, translationY, length, Easing.CubicIn);
            }

            element.TranslationX = translationX;
            element.TranslationY = translationY;

            return Task.FromResult(0);
        }

        /// <summary>
        /// Returns the root element that caused the operation.
        /// </summary>
        public VisualElement RootElement { get; }

        /// <summary>
        /// Returns a list of all elements that exists for the operation.
        /// </summary>
        public IReadOnlyList<VisualElement> Elements;

        /// <summary>
        /// The direction that the layout operation was performed in.
        /// </summary>
        public LayoutDirection Direction { get; }

        /// <summary>
        /// The value that the element was affected by in the layout direction.
        /// </summary>
        public double Value { get; }
    }
}