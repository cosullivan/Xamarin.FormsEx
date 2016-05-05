using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Xamarin.FormsEx
{
    public sealed class Flyout
    {
        public const uint Instant = 0;
        public const uint Fast = 75;
        public const uint Normal = 250;
        public const uint Slow = 1000;

        public static readonly BindableProperty PositionProperty =
            BindableProperty.CreateAttached(
                "Position",
                typeof (FlyoutPosition),
                typeof (Flyout),
                FlyoutPosition.Left);

        public static readonly BindableProperty PinToProperty =
            BindableProperty.CreateAttached(
                "PinTo",
                typeof (VisualElement),
                typeof (Flyout),
                null,
                propertyChanged: OnPinToChanged);

        static readonly BindablePropertyKey PinnedPropertyKey =
            BindableProperty.CreateReadOnly(
                "Pinned",
                typeof (IReadOnlyList<VisualElement>),
                typeof (Flyout),
                null);

        public static readonly BindableProperty PinnedProperty = PinnedPropertyKey.BindableProperty;

        /// <summary>
        /// Called when the PinTo value changes.
        /// </summary>
        /// <param name="bindable">The instance that the has had its value set.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        static void OnPinToChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var element = bindable as VisualElement;

            if (element == null)
            {
                return;
            }

            if (oldValue is VisualElement)
            {
                GetPinned((VisualElement) oldValue).Remove(element);
            }

            if (newValue is VisualElement)
            {
                GetPinned((VisualElement) newValue).Add(element);
            }
        }

        /// <summary>
        /// Gets the list of pinned elements for the bindable object.
        /// </summary>
        /// <param name="bindableObject">The bindable object to get the list of pinned elements.</param>
        /// <returns>The list of pinned elements of the bindable object.</returns>
        internal static List<VisualElement> GetPinned(BindableObject bindableObject)
        {
            if (bindableObject == null)
            {
                throw new ArgumentNullException(nameof(bindableObject));
            }

            var value = bindableObject.GetValue(PinnedPropertyKey.BindableProperty) as List<VisualElement>;

            if (value == null)
            {
                value = new List<VisualElement>();
                bindableObject.SetValue(PinnedPropertyKey, value);
            }

            return value;
        }

        /// <summary>
        /// Gets the position for the object.
        /// </summary>
        /// <param name="bindableObject">The bindable object to get the position for.</param>
        /// <returns>The position that has been assigned to the given object.</returns>
        public static FlyoutPosition GetPosition(BindableObject bindableObject)
        {
            if (bindableObject == null)
            {
                throw new ArgumentNullException(nameof(bindableObject));
            }

            return (FlyoutPosition) bindableObject.GetValue(PositionProperty);
        }

        /// <summary>
        /// Sets the position for the object.
        /// </summary>
        /// <param name="bindableObject">The bindable object to set the position for.</param>
        /// <param name="position">The position that the view should be flown out from.</param>
        /// <returns>The position that has been assigned to the given object.</returns>
        public static void SetPosition(BindableObject bindableObject, FlyoutPosition position)
        {
            if (bindableObject == null)
            {
                throw new ArgumentNullException(nameof(bindableObject));
            }

            bindableObject.SetValue(PositionProperty, position);
        }

        /// <summary>
        /// Gets the element that the instance is pinned to.
        /// </summary>
        /// <param name="bindableObject">The bindable object to get the pin target for.</param>
        /// <returns>The element that the instance is pinned to.</returns>
        public static VisualElement GetPinTo(BindableObject bindableObject)
        {
            if (bindableObject == null)
            {
                throw new ArgumentNullException(nameof(bindableObject));
            }

            return (VisualElement) bindableObject.GetValue(PinToProperty);
        }
    }

    public static class FlyoutExtensions
    {
        /// <summary>
        /// Perform the flyout for the element.
        /// </summary>
        /// <param name="element">The element to perform the flyout for.</param>
        /// <returns>A task which asynchronously performs the operation.</returns>
        public static Task FlyoutAsync(this VisualElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            return FlyoutAsync(element, Flyout.Normal);
        }

        /// <summary>
        /// Perform the flyout for the element.
        /// </summary>
        /// <param name="element">The element to perform the flyout for.</param>
        /// <param name="length">The speed in which to perform the animation for the flyout.</param>
        /// <returns>A task which asynchronously performs the operation.</returns>
        public static Task FlyoutAsync(this VisualElement element, uint length)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            var container = LayoutOperation.GetContainer(element);

            switch (Flyout.GetPosition(element))
            {
                case FlyoutPosition.Left:
                    return TranslateRightAsync(container, element, length);

                case FlyoutPosition.Right:
                    return TranslateLeftAsync(container, element, length);

                case FlyoutPosition.Top:
                    return TranslateDownAsync(container, element, length);

                case FlyoutPosition.Bottom:
                    return TranslateUpAsync(container, element, length);
            }

            throw new NotSupportedException($"The Flyout Position '{Flyout.GetPosition(element)}' is not currently supported.");
        }

        /// <summary>
        /// Returns all the elements that will be affected by a layout operation on the given root element.
        /// </summary>
        /// <param name="rootElement">The root element to determine the pinned hierarchy from.</param>
        /// <returns>The list of visual elements that will be affectect by a layout to the root element.</returns>
        static IEnumerable<VisualElement> CalculateAffectedElements(VisualElement rootElement)
        {
            var hash = new HashSet<VisualElement>();

            CalculateAffectedElements(rootElement, hash);

            return hash;
        }

        /// <summary>
        /// Adds the effected elements to the given hash.
        /// </summary>
        /// <param name="rootElement">The root element to add the effected elements to.</param>
        /// <param name="hash">The hash to track the visitation of the elements.</param>
        static void CalculateAffectedElements(VisualElement rootElement, HashSet<VisualElement> hash)
        {
            foreach (var element in Flyout.GetPinned(rootElement))
            {
                if (hash.Contains(element))
                {
                    continue;
                }

                hash.Add(element);
                CalculateAffectedElements(element, hash);
            }
        }

        /// <summary>
        /// Perform a Flyout in an Up direction.
        /// </summary>
        /// <param name="layout">The parent layout to perform the flyout relative to.</param>
        /// <param name="element">The element that is to be moved.</param>
        /// <param name="length">The length of the animation.</param>
        /// <returns>A task which asynchronously performs the operation.</returns>
        static Task TranslateUpAsync(View layout, VisualElement element, uint length)
        {
            var translationY = element.Bounds.Height - (layout.Bounds.Bottom - element.Bounds.Y - element.TranslationY);

            return TranslateAsync(element, length, LayoutDirection.Vertical, -translationY);
        }

        /// <summary>
        /// Perform a Flyout in an down direction.
        /// </summary>
        /// <param name="layout">The parent layout to perform the flyout relative to.</param>
        /// <param name="element">The element that is to be moved.</param>
        /// <param name="length">The length of the animation.</param>
        /// <returns>A task which asynchronously performs the operation.</returns>
        static Task TranslateDownAsync(View layout, VisualElement element, uint length)
        {
            var translationY = element.Bounds.Height - (element.Bounds.Bottom - layout.Bounds.Top - element.TranslationY);

            return TranslateAsync(element, length, LayoutDirection.Vertical, translationY);
        }

        /// <summary>
        /// Perform a flyout to the right.
        /// </summary>
        /// <param name="layout">The parent layout to perform the flyout relative to.</param>
        /// <param name="element">The element to perform the flyout for.</param>
        /// <param name="length">The speed in which to perform the animation for the flyout.</param>
        /// <returns>A task which asynchronously performs the operation.</returns>
        static Task TranslateRightAsync(View layout, VisualElement element, uint length)
        {
            var translationX = element.Bounds.Width - (element.Bounds.Right - layout.Bounds.Left - element.TranslationX);

            return TranslateAsync(element, length, LayoutDirection.Horizontal, translationX);
        }

        /// <summary>
        /// Perform a flyout to the right.
        /// </summary>
        /// <param name="layout">The parent layout to perform the flyout relative to.</param>
        /// <param name="element">The element to perform the flyout for.</param>
        /// <param name="length">The speed in which to perform the animation for the flyout.</param>
        /// <returns>A task which asynchronously performs the operation.</returns>
        static Task TranslateLeftAsync(View layout, VisualElement element, uint length)
        {
            var translationX = element.Bounds.Width - (layout.Bounds.Right - element.Bounds.Left - element.TranslationX);

            return TranslateAsync(element, length, LayoutDirection.Horizontal, -translationX);
        }

        /// <summary>
        /// Apply the layout operation to the given element.
        /// </summary>
        /// <param name="element">The element to apply the translation to.</param>
        /// <param name="length">The speed of the animation.</param>
        /// <param name="direction">The direction in which to perform the operation.</param>
        /// <param name="value">The amount to which to apply the translation.</param>
        /// <returns>A task which asynchronously performs the operation.</returns>
        static Task TranslateAsync(VisualElement element, uint length, LayoutDirection direction, double value)
        {
            if (Math.Abs(value) < Double.Epsilon)
            {
                return Task.FromResult(0);
            }

            var elements = new[] { element }.Union(CalculateAffectedElements(element)).ToList();

            var tasks = elements.Select(e =>
            {
                LayoutOperation.Push(new LayoutOperation(e, direction, value));

                return LayoutOperation.TranslateAsync(e, length);
            });

            return Task.WhenAll(tasks);
        }

        /// <summary>
        /// Move the flyout back to the previous position.
        /// </summary>
        /// <param name="element">The element to move back on.</param>
        /// <returns>A task which asynchronously performs the operation.</returns>
        public static Task BackAsync(this VisualElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            return BackAsync(element, Flyout.Normal);
        }

        /// <summary>
        /// Move the flyout back to the previous position.
        /// </summary>
        /// <param name="element">The element to move back on.</param>
        /// <param name="length">The speed in which to perform the animation.</param>
        /// <returns>A task which asynchronously performs the operation.</returns>
        public static Task BackAsync(this VisualElement element, uint length)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            LayoutOperation.Pop(element);

            HERE: how to handle batches??

            return LayoutOperation.TranslateAsync(element, length);
        }

        /// <summary>
        /// Move the flyout back to the previous position.
        /// </summary>
        /// <param name="element">The element to move back on.</param>
        /// <returns>A task which asynchronously performs the operation.</returns>
        public static Task CloseFlyoutAsync(this VisualElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            return Task.FromResult(0);
        }

        /// <summary>
        /// Move the flyout back to the previous position.
        /// </summary>
        /// <param name="element">The element to move back on.</param>
        /// <param name="length">The speed in which to perform the animation.</param>
        /// <returns>A task which asynchronously performs the operation.</returns>
        public static Task CloseFlyoutAsync(this VisualElement element, uint length)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            return Task.FromResult(0);
        }
    }
}