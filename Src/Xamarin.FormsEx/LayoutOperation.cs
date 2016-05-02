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
                typeof(Stack<LayoutOperation>),
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
        /// Returns the LayoutOperation that is currently applied to the bindable object.
        /// </summary>
        /// <param name="bindableObject">The instance to return the layout operation for.</param>
        /// <returns>The layout operation that is assigned to the instance.</returns>
        internal static Stack<LayoutOperation> GetLayoutOperations(BindableObject bindableObject)
        {
            if (bindableObject == null)
            {
                throw new ArgumentNullException(nameof(bindableObject));
            }

            var operations = bindableObject.GetValue(LayoutOperationsPropertyKey.BindableProperty) as Stack<LayoutOperation>;

            if (operations == null)
            {
                operations = new Stack<LayoutOperation>();
                bindableObject.SetValue(LayoutOperationsPropertyKey, operations);
            }

            return operations;
        }

        ///// <summary>
        ///// Create the translation point for the current operation.
        ///// </summary>
        ///// <returns>The point that represents the translation point for the operation.</returns>
        //Point CreateTranslationPoint()
        //{
        //    var translationX = 0.0;
        //    var translationY = 0.0;

        //    switch (Direction)
        //    {
        //        case LayoutDirection.Horizontal:
        //            translationX += Value;
        //            break;

        //        case LayoutDirection.Vertical:
        //            translationY += Value;
        //            break;
        //    }

        //    return new Point(translationX, translationY);
        //}

        /// <summary>
        /// Calculate the translation point for the given element.
        /// </summary>
        /// <param name="element">The element to calculation the translation point for.</param>
        /// <returns>The point at which to translate the element to.</returns>
        static Point CalculateTranslationPoint(VisualElement element)
        {
            var translationX = 0.0;
            var translationY = 0.0;

            foreach (var operation in GetLayoutOperations(element))
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

            var point = CalculateTranslationPoint(RootElement);

            var tasks = new[] { RootElement }.Union(OtherElements).Select(element => TranslateToAsync(element, point.X, point.Y, length));

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
                return element.TranslateTo(element.TranslationX + translationX, element.TranslationY + translationY, length, Easing.CubicIn);
            }

            element.TranslationX = element.TranslationX + translationX;
            element.TranslationY = element.TranslationY + translationY;

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