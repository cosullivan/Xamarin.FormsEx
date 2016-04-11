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
            BindableProperty.CreateAttached<Flyout, FlyoutPosition>(
                bindable => Flyout.GetPosition(bindable), FlyoutPosition.Left);

        static readonly BindablePropertyKey IsShowingPropertyKey =
            BindableProperty.CreateAttachedReadOnly<Flyout, bool>(
                bindable => Flyout.GetIsShowing(bindable), false);

        public static readonly BindableProperty IsShowingProperty = IsShowingPropertyKey.BindableProperty;

        static readonly BindablePropertyKey StatePropertyKey =
            BindableProperty.CreateAttachedReadOnly<Flyout, Stack<Point>>(
                bindable => Flyout.GetState(bindable), null, defaultValueCreator: CreateInstance<Stack<Point>>);

        static readonly BindablePropertyKey PinnedPropertyKey =
            BindableProperty.CreateAttachedReadOnly<Flyout, List<VisualElement>>(
                bindable => Flyout.GetPinned(bindable), null, defaultValueCreator: CreateInstance<List<VisualElement>>);

        public static readonly BindableProperty PinToProperty =
            BindableProperty.CreateAttached<Flyout, VisualElement>(
                bindable => Flyout.GetPinTo(bindable), null, propertyChanged: OnPinToChanged);

        /// <summary>
        /// Called when the PinTo value changes.
        /// </summary>
        /// <param name="bindable">The instance that the has had its value set.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        static void OnPinToChanged(BindableObject bindable, VisualElement oldValue, VisualElement newValue)
        {
            var element = bindable as VisualElement;

            if (element == null)
            {
                return;
            }

            if (oldValue != null)
            {
                GetPinned(oldValue).Remove(element);
            }

            if (newValue != null)
            {
                GetPinned(newValue).Add(element);
            }
        }

        /// <summary>
        /// Create a default instance for the bindable object.
        /// </summary>
        /// <param name="bindableObject">The bindable object to create the default instance for.</param>
        /// <returns>The default instance to use for the bindable object.</returns>
        static T CreateInstance<T>(BindableObject bindableObject) where T : new()
        {
            return new T();
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

            return (FlyoutPosition)bindableObject.GetValue(PositionProperty);
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

            return (VisualElement)bindableObject.GetValue(PinToProperty);
        }

        /// <summary>
        /// Gets a value indicating whether or not the flyout is showing.
        /// </summary>
        /// <param name="bindableObject">The bindable object to get the position for.</param>
        /// <returns>The value that represents whether flyout is currently showing.</returns>
        public static bool GetIsShowing(BindableObject bindableObject)
        {
            if (bindableObject == null)
            {
                throw new ArgumentNullException(nameof(bindableObject));
            }

            return (bool)bindableObject.GetValue(IsShowingProperty);
        }

        /// <summary>
        /// Sets a value indicating whether or not the flyout is currently shoing.
        /// </summary>
        /// <param name="bindableObject">The bindable object to set the value on.</param>
        /// <param name="isShowing">A value indicating whether or not the flyout is showing.</param>
        internal static void SetIsShowing(BindableObject bindableObject, bool isShowing)
        {
            if (bindableObject == null)
            {
                throw new ArgumentNullException(nameof(bindableObject));
            }

            bindableObject.SetValue(IsShowingPropertyKey, isShowing);
        }

        /// <summary>
        /// Gets the state for the bindable object.
        /// </summary>
        /// <param name="bindableObject">The bindable object to get the state for.</param>
        /// <returns>The state of the bindable object.</returns>
        internal static Stack<Point> GetState(BindableObject bindableObject)
        {
            if (bindableObject == null)
            {
                throw new ArgumentNullException(nameof(bindableObject));
            }

            return (Stack<Point>)bindableObject.GetValue(StatePropertyKey.BindableProperty);
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

            return (List<VisualElement>)bindableObject.GetValue(PinnedPropertyKey.BindableProperty);
        }
    }

    public static class FlyoutExtensions
    {
        /// <summary>
        /// Gets the relative layout that the element is contained within.
        /// </summary>
        /// <param name="element">The element to get the parent layout for.</param>
        /// <returns>The relative layout that is the parent of the given element.</returns>
        static RelativeLayout GetLayout(Element element)
        {
            if (element == null)
            {
                throw new InvalidOperationException("Can not find the parent layout for the given element.");
            }

            if (element.ParentView is RelativeLayout)
            {
                return (RelativeLayout)element.ParentView;
            }

            return GetLayout(element.ParentView);
        }

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

            Flyout.GetState(element).Push(new Point(element.TranslationX, element.TranslationY));
            Flyout.SetIsShowing(element, true);

            switch (Flyout.GetPosition(element))
            {
                case FlyoutPosition.Left:
                    return AnimateRightAsync(GetLayout(element), element, length);

                case FlyoutPosition.Right:
                    return AnimateLeftAsync(GetLayout(element), element, length);

                case FlyoutPosition.Top:
                    return AnimateDownAsync(GetLayout(element), element, length);

                case FlyoutPosition.Bottom:
                    return AnimateUpAsync(GetLayout(element), element, length);
            }

            throw new NotSupportedException($"The Flyout Position '{Flyout.GetPosition(element)}' is not currently supported.");
        }

        /// <summary>
        /// Perform a Flyout in an Up direction.
        /// </summary>
        /// <param name="layout">The parent layout to perform the flyout relative to.</param>
        /// <param name="element">The element that is to be moved.</param>
        /// <param name="length">The length of the animation.</param>
        /// <returns>A task which asynchronously performs the operation.</returns>
        static Task AnimateUpAsync(View layout, VisualElement element, uint length)
        {
            var translationY = element.Bounds.Height - (layout.Bounds.Bottom - element.Bounds.Y);

            return AnimateTranslationAsync(element, 0, -translationY, length);
        }

        /// <summary>
        /// Perform a Flyout in an down direction.
        /// </summary>
        /// <param name="layout">The parent layout to perform the flyout relative to.</param>
        /// <param name="element">The element that is to be moved.</param>
        /// <param name="length">The length of the animation.</param>
        /// <returns>A task which asynchronously performs the operation.</returns>
        static Task AnimateDownAsync(View layout, VisualElement element, uint length)
        {
            var translationY = element.Bounds.Height - (element.Bounds.Bottom - layout.Bounds.Top);

            return AnimateTranslationAsync(element, 0, translationY, length);
        }

        /// <summary>
        /// Perform a flyout to the right.
        /// </summary>
        /// <param name="layout">The parent layout to perform the flyout relative to.</param>
        /// <param name="element">The element to perform the flyout for.</param>
        /// <param name="length">The speed in which to perform the animation for the flyout.</param>
        /// <returns>A task which asynchronously performs the operation.</returns>
        static Task AnimateRightAsync(View layout, VisualElement element, uint length)
        {
            var translationX = element.Bounds.Width - (element.Bounds.Right - layout.Bounds.Left);

            return AnimateTranslationAsync(element, translationX, 0, length);
        }

        /// <summary>
        /// Perform a flyout to the right.
        /// </summary>
        /// <param name="layout">The parent layout to perform the flyout relative to.</param>
        /// <param name="element">The element to perform the flyout for.</param>
        /// <param name="length">The speed in which to perform the animation for the flyout.</param>
        /// <returns>A task which asynchronously performs the operation.</returns>
        static Task AnimateLeftAsync(View layout, VisualElement element, uint length)
        {
            var translationX = element.Bounds.Width - (layout.Bounds.Right - element.Bounds.Left);

            return AnimateTranslationAsync(element, -translationX, 0, length);
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

            var state = Flyout.GetState(element);
            Flyout.SetIsShowing(element, state.Count > 1);

            if (state.Count == 0)
            {
                return Task.FromResult(0);
            }

            var top = state.Pop();
            Flyout.SetIsShowing(element, state.Count > 0);

            return AnimateTranslationAsync(element, top.X, top.Y, length);
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

            return CloseFlyoutAsync(element, Flyout.Normal);
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

            if (Flyout.GetState(element).Count == 0)
            {
                return Task.FromResult(0);
            }

            Flyout.GetState(element).Clear();
            Flyout.SetIsShowing(element, false);

            return AnimateTranslationAsync(element, 0, 0, length);
        }
        
        /// <summary>
        /// Perform a flyout on a horizontal basis.
        /// </summary>
        /// <param name="element">The element to perform the flyout for.</param>
        /// <param name="translationX">The translation amount to animate on the X scale.</param>
        /// <param name="translationY">The translation amount to animate on the Y scale.</param>
        /// <param name="length">The speed in which to perform the animation for the flyout.</param>
        /// <returns>A task which asynchronously performs the operation.</returns>
        static Task AnimateTranslationAsync(VisualElement element, double translationX, double translationY, uint length)
        {
            var elements = Flyout.GetPinned(element).Union(new[] { element });

            var tasks = elements.Select(e => ExecuteTranslationAsync(e, translationX, translationY, length));

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
        static Task ExecuteTranslationAsync(VisualElement element, double translationX, double translationY, uint length)
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
    }
}
