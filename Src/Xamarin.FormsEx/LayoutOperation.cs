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
        /// <param name="direction">The direction that the layout should be performed in.</param>
        /// <param name="value">The value that the elements will be affected by in the layout direction.</param>
        /// <param name="otherElements">The list of other elements that are to be affected by the layout operation.</param>
        internal LayoutOperation(VisualElement element, LayoutDirection direction, double value, IEnumerable<VisualElement> otherElements)
        {
            RootElement = element;
            Direction = direction;
            Value = value;
            OtherElements = otherElements.ToList();
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
        internal static void Push(LayoutOperation operation)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            GetLayoutOperations(GetContainer(operation.RootElement)).Add(operation);
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
        /// Calculate the translation point for the given element.
        /// </summary>
        /// <param name="element">The element to calculation the translation point for.</param>
        /// <returns>The point at which to translate the element to.</returns>
        static Point CalculateTranslationPoint(VisualElement element)
        {
            var translationX = 0.0;
            var translationY = 0.0;

            foreach (var operation in GetLayoutOperations(GetContainer(element)).Where(op => op.ContainsElement(element)))
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
        /// Returns a value indicating whether or not the current operation contains the given element.
        /// </summary>
        /// <param name="element">The element to determine if it exists in the current operation.</param>
        /// <returns>true if the element exists in the operation, false if not.</returns>
        bool ContainsElement(VisualElement element)
        {
            return RootElement == element || OtherElements.Contains(element);
        }

        /// <summary>
        /// Returns a value indicating whether theroot element can be translated with the current operation.
        /// </summary>
        /// <param name="element">The element to test whether it can be translated.</param>
        /// <returns>true if the root can be translated with the operation, false if not.</returns>
        static bool CanTranslate(VisualElement element)
        {
            var point = CalculateTranslationPoint(element);

            return Math.Abs(element.TranslationX - point.X) > Double.Epsilon || Math.Abs(element.TranslationY - point.Y) > Double.Epsilon;
        }

        /// <summary>
        /// Perform the translation for the operation.
        /// </summary>
        /// <param name="length">The speed of the translation.</param>
        /// <returns>A task which asynchronously performs the operation.</returns>
        internal Task TranslateToAsync(uint length)
        {
            if (CanTranslate(RootElement) == false)
            {
                return Task.FromResult(0);
            }

            var tasks = new[] { RootElement }.Union(OtherElements).Select(element =>
            {
                var point = CalculateTranslationPoint(element);

                return TranslateToAsync(element, point.X, point.Y, length);
            });

            return Task.WhenAll(tasks);
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
        /// The direction that the layout operation was performed in.
        /// </summary>
        public LayoutDirection Direction { get; }

        /// <summary>
        /// The value that the elements were affected by in the layout direction.
        /// </summary>
        public double Value { get; }

        /// <summary>
        /// The list of elements that were affected during this operation.
        /// </summary>
        public IReadOnlyList<VisualElement> OtherElements { get; }
    }
}