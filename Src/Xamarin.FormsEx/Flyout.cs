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

        //static readonly BindablePropertyKey LayoutOperationPropertyKey =
        //    BindableProperty.CreateAttachedReadOnly(
        //        "LayoutOperation",
        //        typeof (LayoutOperation),
        //        typeof (Flyout),
        //        null);

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

        ///// <summary>
        ///// Gets the current layout operation that has been applied to the element.
        ///// </summary>
        ///// <param name="bindableObject">The bindable object to get the pin target for.</param>
        ///// <returns>The layout operation that is currently applied to the given element..</returns>
        //internal static LayoutOperation GetLayoutOperation(BindableObject bindableObject)
        //{
        //    if (bindableObject == null)
        //    {
        //        throw new ArgumentNullException(nameof(bindableObject));
        //    }

        //    return bindableObject.GetValue(LayoutOperationPropertyKey.BindableProperty) as LayoutOperation;
        //}

        ///// <summary>
        ///// Sets the layout operation for the given element.
        ///// </summary>
        ///// <param name="bindableObject">The bindable object to set the layout operation on.</param>
        ///// <param name="layoutOperation">The layout operation to set on the element.</param>
        //internal static void SetLayoutOperation(BindableObject bindableObject, LayoutOperation layoutOperation)
        //{
        //    if (bindableObject == null)
        //    {
        //        throw new ArgumentNullException(nameof(bindableObject));
        //    }

        //    bindableObject.SetValue(LayoutOperationPropertyKey, layoutOperation);
        //}
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

            if (element.Parent is RelativeLayout)
            {
                return (RelativeLayout)element.Parent;
            }

            return GetLayout(element.Parent);
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

            switch (Flyout.GetPosition(element))
            {
                //case FlyoutPosition.Left:
                //    return AnimateRightAsync(GetLayout(element), element, length);

                //case FlyoutPosition.Right:
                //    return AnimateLeftAsync(GetLayout(element), element, length);

                //case FlyoutPosition.Top:
                //    return AnimateDownAsync(GetLayout(element), element, length);

                case FlyoutPosition.Bottom:
                    return AnimateUpAsync(GetLayout(element), element, length);
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
        static Task AnimateUpAsync(View layout, VisualElement element, uint length)
        {
            var translationY = element.Bounds.Height - (layout.Bounds.Bottom - element.Bounds.Y);

            return TranslateAsync(element, length, LayoutDirection.Vertical, -translationY);
        }

        static Task TranslateAsync(VisualElement element, uint length, LayoutDirection direction, double value)
        {
            HERE: move the SetLayoutOperaiton onto the LayoutOPeration class

            var operation = new LayoutOperation(element, direction, value, CalculateAffectedElements(element));

            Flyout.SetLayoutOperation(element, operation);

            return operation.TranslateToAsync(length);
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

            return Task.FromResult(0);
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





    //    public static readonly BindableProperty PositionProperty =
    //        BindableProperty.CreateAttached<Flyout, FlyoutPosition>(
    //            bindable => Flyout.GetPosition(bindable), FlyoutPosition.Left);

    //    static readonly BindablePropertyKey IsShowingPropertyKey =
    //        BindableProperty.CreateAttachedReadOnly<Flyout, bool>(
    //            bindable => Flyout.GetIsShowing(bindable), false);

    //    public static readonly BindableProperty IsShowingProperty = IsShowingPropertyKey.BindableProperty;

    //    static readonly BindablePropertyKey StatePropertyKey =
    //        BindableProperty.CreateAttachedReadOnly<Flyout, Stack<Point>>(
    //            bindable => Flyout.GetState(bindable), null, defaultValueCreator: CreateInstance<Stack<Point>>);

    //    static readonly BindablePropertyKey PinnedPropertyKey =
    //        BindableProperty.CreateAttachedReadOnly<Flyout, List<VisualElement>>(
    //            bindable => Flyout.GetPinned(bindable), null, defaultValueCreator: CreateInstance<List<VisualElement>>);

    //    public static readonly BindableProperty PinToProperty =
    //        BindableProperty.CreateAttached<Flyout, VisualElement>(
    //            bindable => Flyout.GetPinTo(bindable), null, propertyChanged: OnPinToChanged);

    //    /// <summary>
    //    /// Called when the PinTo value changes.
    //    /// </summary>
    //    /// <param name="bindable">The instance that the has had its value set.</param>
    //    /// <param name="oldValue">The old value.</param>
    //    /// <param name="newValue">The new value.</param>
    //    static void OnPinToChanged(BindableObject bindable, VisualElement oldValue, VisualElement newValue)
    //    {
    //        var element = bindable as VisualElement;

    //        if (element == null)
    //        {
    //            return;
    //        }

    //        if (oldValue != null)
    //        {
    //            GetPinned(oldValue).Remove(element);
    //        }

    //        if (newValue != null)
    //        {
    //            GetPinned(newValue).Add(element);
    //        }
    //    }

    //    /// <summary>
    //    /// Create a default instance for the bindable object.
    //    /// </summary>
    //    /// <param name="bindableObject">The bindable object to create the default instance for.</param>
    //    /// <returns>The default instance to use for the bindable object.</returns>
    //    static T CreateInstance<T>(BindableObject bindableObject) where T : new()
    //    {
    //        return new T();
    //    }

    //    /// <summary>
    //    /// Gets the position for the object.
    //    /// </summary>
    //    /// <param name="bindableObject">The bindable object to get the position for.</param>
    //    /// <returns>The position that has been assigned to the given object.</returns>
    //    public static FlyoutPosition GetPosition(BindableObject bindableObject)
    //    {
    //        if (bindableObject == null)
    //        {
    //            throw new ArgumentNullException(nameof(bindableObject));
    //        }

    //        return (FlyoutPosition)bindableObject.GetValue(PositionProperty);
    //    }

    //    /// <summary>
    //    /// Sets the position for the object.
    //    /// </summary>
    //    /// <param name="bindableObject">The bindable object to set the position for.</param>
    //    /// <param name="position">The position that the view should be flown out from.</param>
    //    /// <returns>The position that has been assigned to the given object.</returns>
    //    public static void SetPosition(BindableObject bindableObject, FlyoutPosition position)
    //    {
    //        if (bindableObject == null)
    //        {
    //            throw new ArgumentNullException(nameof(bindableObject));
    //        }

    //        bindableObject.SetValue(PositionProperty, position);
    //    }

    //    /// <summary>
    //    /// Gets the element that the instance is pinned to.
    //    /// </summary>
    //    /// <param name="bindableObject">The bindable object to get the pin target for.</param>
    //    /// <returns>The element that the instance is pinned to.</returns>
    //    public static VisualElement GetPinTo(BindableObject bindableObject)
    //    {
    //        if (bindableObject == null)
    //        {
    //            throw new ArgumentNullException(nameof(bindableObject));
    //        }

    //        return (VisualElement)bindableObject.GetValue(PinToProperty);
    //    }

    //    /// <summary>
    //    /// Gets a value indicating whether or not the flyout is showing.
    //    /// </summary>
    //    /// <param name="bindableObject">The bindable object to get the position for.</param>
    //    /// <returns>The value that represents whether flyout is currently showing.</returns>
    //    public static bool GetIsShowing(BindableObject bindableObject)
    //    {
    //        if (bindableObject == null)
    //        {
    //            throw new ArgumentNullException(nameof(bindableObject));
    //        }

    //        return (bool)bindableObject.GetValue(IsShowingProperty);
    //    }

    //    /// <summary>
    //    /// Sets a value indicating whether or not the flyout is currently shoing.
    //    /// </summary>
    //    /// <param name="bindableObject">The bindable object to set the value on.</param>
    //    /// <param name="isShowing">A value indicating whether or not the flyout is showing.</param>
    //    internal static void SetIsShowing(BindableObject bindableObject, bool isShowing)
    //    {
    //        if (bindableObject == null)
    //        {
    //            throw new ArgumentNullException(nameof(bindableObject));
    //        }

    //        bindableObject.SetValue(IsShowingPropertyKey, isShowing);
    //    }

    //    /// <summary>
    //    /// Gets the state for the bindable object.
    //    /// </summary>
    //    /// <param name="bindableObject">The bindable object to get the state for.</param>
    //    /// <returns>The state of the bindable object.</returns>
    //    internal static Stack<Point> GetState(BindableObject bindableObject)
    //    {
    //        if (bindableObject == null)
    //        {
    //            throw new ArgumentNullException(nameof(bindableObject));
    //        }

    //        return (Stack<Point>)bindableObject.GetValue(StatePropertyKey.BindableProperty);
    //    }

    //    /// <summary>
    //    /// Gets the list of pinned elements for the bindable object.
    //    /// </summary>
    //    /// <param name="bindableObject">The bindable object to get the list of pinned elements.</param>
    //    /// <returns>The list of pinned elements of the bindable object.</returns>
    //    internal static List<VisualElement> GetPinned(BindableObject bindableObject)
    //    {
    //        if (bindableObject == null)
    //        {
    //            throw new ArgumentNullException(nameof(bindableObject));
    //        }

    //        return (List<VisualElement>)bindableObject.GetValue(PinnedPropertyKey.BindableProperty);
    //    }
    //}

    //public static class FlyoutExtensions
    //{
    //    /// <summary>
    //    /// Gets the relative layout that the element is contained within.
    //    /// </summary>
    //    /// <param name="element">The element to get the parent layout for.</param>
    //    /// <returns>The relative layout that is the parent of the given element.</returns>
    //    static RelativeLayout GetLayout(Element element)
    //    {
    //        if (element == null)
    //        {
    //            throw new InvalidOperationException("Can not find the parent layout for the given element.");
    //        }

    //        if (element.Parent is RelativeLayout)
    //        {
    //            return (RelativeLayout)element.Parent;
    //        }

    //        return GetLayout(element.Parent);
    //    }

    //    /// <summary>
    //    /// Perform the flyout for the element.
    //    /// </summary>
    //    /// <param name="element">The element to perform the flyout for.</param>
    //    /// <returns>A task which asynchronously performs the operation.</returns>
    //    public static Task FlyoutAsync(this VisualElement element)
    //    {
    //        if (element == null)
    //        {
    //            throw new ArgumentNullException(nameof(element));
    //        }

    //        return FlyoutAsync(element, Flyout.Normal);
    //    }

    //    /// <summary>
    //    /// Perform the flyout for the element.
    //    /// </summary>
    //    /// <param name="element">The element to perform the flyout for.</param>
    //    /// <param name="length">The speed in which to perform the animation for the flyout.</param>
    //    /// <returns>A task which asynchronously performs the operation.</returns>
    //    public static Task FlyoutAsync(this VisualElement element, uint length)
    //    {
    //        if (element == null)
    //        {
    //            throw new ArgumentNullException(nameof(element));
    //        }

    //        //Flyout.GetState(element).Push(new Point(element.TranslationX, element.TranslationY));
    //        Flyout.SetIsShowing(element, true);

    //        switch (Flyout.GetPosition(element))
    //        {
    //            case FlyoutPosition.Left:
    //                return AnimateRightAsync(GetLayout(element), element, length);

    //            case FlyoutPosition.Right:
    //                return AnimateLeftAsync(GetLayout(element), element, length);

    //            case FlyoutPosition.Top:
    //                return AnimateDownAsync(GetLayout(element), element, length);

    //            case FlyoutPosition.Bottom:
    //                return AnimateUpAsync(GetLayout(element), element, length);
    //        }

    //        throw new NotSupportedException($"The Flyout Position '{Flyout.GetPosition(element)}' is not currently supported.");
    //    }

    //    /// <summary>
    //    /// Perform a Flyout in an Up direction.
    //    /// </summary>
    //    /// <param name="layout">The parent layout to perform the flyout relative to.</param>
    //    /// <param name="element">The element that is to be moved.</param>
    //    /// <param name="length">The length of the animation.</param>
    //    /// <returns>A task which asynchronously performs the operation.</returns>
    //    static Task AnimateUpAsync(View layout, VisualElement element, uint length)
    //    {
    //        var translationY = element.Bounds.Height - (layout.Bounds.Bottom - element.Bounds.Y);

    //        //if (Math.Abs(element.TranslationY - translationY) < Double.Epsilon)
    //        //{
    //        //    return Task.FromResult(0);
    //        //}

    //        //Flyout.GetState(element).Push(new Point(0, -translationY));

    //        return AnimateTranslationAsync(element, 0, -translationY, length);
    //    }

    //    /// <summary>
    //    /// Perform a Flyout in an down direction.
    //    /// </summary>
    //    /// <param name="layout">The parent layout to perform the flyout relative to.</param>
    //    /// <param name="element">The element that is to be moved.</param>
    //    /// <param name="length">The length of the animation.</param>
    //    /// <returns>A task which asynchronously performs the operation.</returns>
    //    static Task AnimateDownAsync(View layout, VisualElement element, uint length)
    //    {
    //        var translationY = element.Bounds.Height - (element.Bounds.Bottom - layout.Bounds.Top);

    //        return AnimateTranslationAsync(element, 0, translationY, length);
    //    }

    //    /// <summary>
    //    /// Perform a flyout to the right.
    //    /// </summary>
    //    /// <param name="layout">The parent layout to perform the flyout relative to.</param>
    //    /// <param name="element">The element to perform the flyout for.</param>
    //    /// <param name="length">The speed in which to perform the animation for the flyout.</param>
    //    /// <returns>A task which asynchronously performs the operation.</returns>
    //    static Task AnimateRightAsync(View layout, VisualElement element, uint length)
    //    {
    //        var translationX = element.Bounds.Width - (element.Bounds.Right - layout.Bounds.Left);

    //        return AnimateTranslationAsync(element, translationX, 0, length);
    //    }

    //    /// <summary>
    //    /// Perform a flyout to the right.
    //    /// </summary>
    //    /// <param name="layout">The parent layout to perform the flyout relative to.</param>
    //    /// <param name="element">The element to perform the flyout for.</param>
    //    /// <param name="length">The speed in which to perform the animation for the flyout.</param>
    //    /// <returns>A task which asynchronously performs the operation.</returns>
    //    static Task AnimateLeftAsync(View layout, VisualElement element, uint length)
    //    {
    //        var translationX = element.Bounds.Width - (layout.Bounds.Right - element.Bounds.Left);

    //        return AnimateTranslationAsync(element, -translationX, 0, length);
    //    }

    //    /// <summary>
    //    /// Move the flyout back to the previous position.
    //    /// </summary>
    //    /// <param name="element">The element to move back on.</param>
    //    /// <returns>A task which asynchronously performs the operation.</returns>
    //    public static Task BackAsync(this VisualElement element)
    //    {
    //        if (element == null)
    //        {
    //            throw new ArgumentNullException(nameof(element));
    //        }

    //        return BackAsync(element, Flyout.Normal);
    //    }

    //    /// <summary>
    //    /// Move the flyout back to the previous position.
    //    /// </summary>
    //    /// <param name="element">The element to move back on.</param>
    //    /// <param name="length">The speed in which to perform the animation.</param>
    //    /// <returns>A task which asynchronously performs the operation.</returns>
    //    public static Task BackAsync(this VisualElement element, uint length)
    //    {
    //        if (element == null)
    //        {
    //            throw new ArgumentNullException(nameof(element));
    //        }

    //        var state = Flyout.GetState(element);
    //        Flyout.SetIsShowing(element, state.Count > 1);

    //        if (state.Count == 0)
    //        {
    //            return Task.FromResult(0);
    //        }

    //        var top = state.Pop();
    //        Flyout.SetIsShowing(element, state.Count > 0);

    //        return AnimateTranslationAsync(element, top.X, top.Y, length);
    //    }

    //    /// <summary>
    //    /// Move the flyout back to the previous position.
    //    /// </summary>
    //    /// <param name="element">The element to move back on.</param>
    //    /// <returns>A task which asynchronously performs the operation.</returns>
    //    public static Task CloseFlyoutAsync(this VisualElement element)
    //    {
    //        if (element == null)
    //        {
    //            throw new ArgumentNullException(nameof(element));
    //        }

    //        return CloseFlyoutAsync(element, Flyout.Normal);
    //    }

    //    /// <summary>
    //    /// Move the flyout back to the previous position.
    //    /// </summary>
    //    /// <param name="element">The element to move back on.</param>
    //    /// <param name="length">The speed in which to perform the animation.</param>
    //    /// <returns>A task which asynchronously performs the operation.</returns>
    //    public static Task CloseFlyoutAsync(this VisualElement element, uint length)
    //    {
    //        if (element == null)
    //        {
    //            throw new ArgumentNullException(nameof(element));
    //        }

    //        if (Flyout.GetState(element).Count == 0)
    //        {
    //            return Task.FromResult(0);
    //        }

    //        Flyout.GetState(element).Clear();
    //        Flyout.SetIsShowing(element, false);

    //        return AnimateTranslationAsync(element, 0, 0, length);
    //    }

    //    /// <summary>
    //    /// Perform a flyout on a horizontal basis.
    //    /// </summary>
    //    /// <param name="element">The element to perform the flyout for.</param>
    //    /// <param name="translationX">The translation amount to animate on the X scale.</param>
    //    /// <param name="translationY">The translation amount to animate on the Y scale.</param>
    //    /// <param name="length">The speed in which to perform the animation for the flyout.</param>
    //    /// <returns>A task which asynchronously performs the operation.</returns>
    //    static Task AnimateTranslationAsync(VisualElement element, double translationX, double translationY, uint length)
    //    {
    //        if (Math.Abs(element.TranslationX - translationX) < Double.Epsilon && Math.Abs(element.TranslationY - translationY) < Double.Epsilon)
    //        {
    //            return Task.FromResult(0);
    //        }

    //        Position(element, new Point(translationX, translationY));

    //        var elements = Flyout.GetPinned(element).Union(new[] { element });

    //        var tasks = elements.Select(e =>
    //        {
    //            var current = Position(e);

    //            return TranslateToAsync(e, current.X, current.Y, length);
    //        });

    //        return Task.WhenAll(tasks);
    //    }

    //    /// <summary>
    //    /// Perform a flyout on a horizontal basis.
    //    /// </summary>
    //    /// <param name="element">The element to perform the flyout for.</param>
    //    /// <param name="translationX">The translation amount to animate on the X scale.</param>
    //    /// <param name="translationY">The translation amount to animate on the Y scale.</param>
    //    /// <param name="length">The speed in which to perform the animation for the flyout.</param>
    //    /// <returns>A task which asynchronously performs the operation.</returns>
    //    static Task TranslateToAsync(VisualElement element, double translationX, double translationY, uint length)
    //    {
    //        if (Math.Abs(element.TranslationX - translationX) < Double.Epsilon && Math.Abs(element.TranslationY - translationY) < Double.Epsilon)
    //        {
    //            return Task.FromResult(0);
    //        }

    //        if (length > 0)
    //        {
    //            return element.TranslateTo(translationX, translationY, length, Easing.CubicIn);
    //        }

    //        element.TranslationX = translationX;
    //        element.TranslationY = translationY;

    //        return Task.FromResult(0);
    //    }

    //    /// <summary>
    //    /// Gets or sets the position of the element.
    //    /// </summary>
    //    /// <param name="element">The element to get or set the position for.</param>
    //    /// <param name="translation">The translation position to set for the element.</param>
    //    /// <returns>The current position for the element.</returns>
    //    static Point Position(VisualElement element, Point? translation = null)
    //    {
    //        if (translation != null)
    //        {
    //            Flyout.GetState(element).Push(new Point(translation.Value.X, translation.Value.Y));
    //        }

    //        return Flyout.GetState(element).FirstOrDefault();
    //    }
    //   }
}