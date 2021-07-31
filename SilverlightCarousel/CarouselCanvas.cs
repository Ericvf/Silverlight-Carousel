using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace SilverlightCarousel
{
    public static class Extensions
    {
        /// <summary>
        /// Eases the current value to the target value by a factor of <paramref name="easingFactor"/>
        /// </summary>
        /// <param name="currentValue"></param>
        /// <param name="targetValue"></param>
        /// <param name="easingFactor"></param>
        public static void EaseToTarget(this double targetValue, ref double currentValue, double easingFactor)
        {
            // Check if the value hasn't reached it's destination
            if (targetValue != currentValue)
                currentValue += (targetValue - currentValue) / easingFactor;
        }
    }

    /// <summary>
    /// Represents a Canvas control with children rendered in a Carousel layout
    /// </summary>
    public class CarouselCanvas : Canvas
    {
        #region Classes
        /// <summary>
        /// We need to have a seperate class for this because the OriginalSource property
        /// of the RoutedEventArgs class is protected.
        /// </summary>
        public class CarouselItemEventArgs : EventArgs
        {
            /// <summary>
            /// Gets or sets the OriginalSource property.
            /// </summary>
            public UIElement OriginalSource { get; set; }
        }

        #endregion

        #region Delegates and Events

        /// <summary> 
        /// This delegate type can be used to implement events for the carousel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void CarouselEventHandler(object sender, CarouselItemEventArgs e);

        /// <summary>
        /// An event that occurs whenever a new item was selected.
        /// </summary>
        public event CarouselEventHandler SelectingItem;

        /// <summary>
        /// An event that occurs whenever a new item was selected.
        /// </summary>
        public event CarouselEventHandler SelectedItem;

        /// <summary>
        /// An event that occurs whenever an item is de-selected.
        /// </summary>
        public event CarouselEventHandler DeselectedItem;

        #endregion

        #region Consts

        /// <summary>
        /// Represents a full circle in radians
        /// </summary>
        private const double fullCircle = Math.PI * 2;

        /// <summary>
        /// Represents a quarter of a circle in radians
        /// </summary>
        private const double quarterCircle = fullCircle / 4;

        #endregion

        #region Private fields

        /// <summary>
        /// Represents the angle of the segments of each child
        /// </summary>
        private double segmentCircle;

        /// <summary>
        /// Represents the target offset for all segments
        /// </summary>
        private double segmentOffsetTarget;

        /// <summary>
        /// Represents the current offset for all segments
        /// </summary>
        private double segmentOffset;

        /// <summary>
        /// Represents the delta value for all segments
        /// </summary>
        private double segmentDelta;

        /// <summary>
        /// Represents the current speed of rotation,this value should be somewhere between 0 and Rotation
        /// </summary>
        private double currentRotationSpeed = 0;

        /// <summary>
        /// Represents the rotation offset for the carousel
        /// </summary>
        private double rotationOffset = 0;

        /// <summary>
        /// Boolean the indicated if the rotation is paused
        /// </summary>
        private bool rotationPaused = false;

        /// <summary>
        /// Boolean that indicates if the rotation is enabled
        /// </summary>
        private bool rotationEnabled = false;

        /// <summary>
        /// A dictionary that contains a unique ID for each child
        /// </summary>
        private Dictionary<int, UIElement> childrenIndex = new Dictionary<int, UIElement>();


        /// <summary>
        /// Represents the current radius of the Carousel
        /// </summary>
        private double currentRadiusX;

        /// <summary>
        /// Represents the current radius of the Carousel
        /// </summary>
        private double currentRadiusY;

        /// <summary>
        /// The currently selected item
        /// </summary>
        private int currentItem;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the Orientation property that controls the layout orientation.
        /// </summary>
        public Orientation Orientation { get; set; }

        /// <summary>
        /// Gets or sets the horizontal radius of the carousel orientation
        /// </summary>
        public double RadiusX { get; set; }

        /// <summary>
        /// Gets or sets the vertical radius of the carousel orientation
        /// </summary>
        public double RadiusY { get; set; }

        /// <summary>
        /// Gets or sets the delay in the transition
        /// </summary>
        public double TransitionDelay { get; set; }

        /// <summary>
        /// Gets or sets the decelleration 
        /// </summary>
        public double Decelleration { get; set; }

        /// <summary>
        /// Gets or sets the elasticity
        /// </summary>
        public double Elasticity { get; set; }

        /// <summary>
        /// Gets or sets the minimum amount of opacity
        /// </summary>
        public double MinOpacity { get; set; }

        /// <summary>
        /// Gets or sets the minimum scale of each object
        /// </summary>
        public double MinScale { get; set; }

        /// <summary>
        /// Gets or sets the rotation speed
        /// </summary>
        public double RotationSpeed { get; set; }

        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the canvas
        /// </summary>
        public CarouselCanvas()
            : base()
        {
            // Create a loaded event
            this.Loaded += new System.Windows.RoutedEventHandler(CarouselCanvas_Loaded);
        }

        /// <summary>
        /// Loads the canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CarouselCanvas_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // Initialize the default parameters
            this.InitializeDefaultParameters();

            // Populate the children in the canvas
            this.PopulateChildren();

            // Transform all the children at load time, this happens every frame though
            this.TransformChildren();

            // Register a Rendering event that goes off every frame
            CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
        }

        /// <summary>
        /// Initializes the parameters the cannot have the default value
        /// </summary>
        private void InitializeDefaultParameters()
        {
            // Handle null properties
            if (this.RadiusX == 0)
                this.RadiusX = this.Width / 2;

            if (this.RadiusY == 0)
                this.RadiusY = this.Height / 2;
        }

        /// <summary>
        /// Populates all the children in the canvas to the dictionary
        /// </summary>
        private void PopulateChildren()
        {
            // Loop through all the children and create ID's for them
            int totalChildren;
            if ((totalChildren = this.Children.Count) > 0)
            {
                // Reset the index
                this.childrenIndex.Clear();

                // Add the child with its ID
                for (int i = 0; i < totalChildren; i++)
                {
                    this.childrenIndex.Add(i, this.Children[i]);

                    // Make sure the rendertransform origin is in the center of the control so it scales nicely
                    this.Children[i].RenderTransformOrigin = new Point(0.5f, 0.5f);
                }

                // Update the size of segment since it is dependant of the number of children
                this.segmentCircle = fullCircle / totalChildren;
            }
        }

        /// <summary>
        /// Clears the children in the carousel
        /// </summary>
        public void ResetChildren()
        {
            // Clear the childrens index
            this.childrenIndex.Clear();

            // Clear the children
            this.Children.Clear();
        }

        /// <summary>
        /// Adds a child
        /// </summary>
        /// <param name="child"></param>
        public void AddChild(UIElement child)
        {
            // Add the char
            this.Children.Add(child);

            // Populate
            this.PopulateChildren();
        }

        #endregion

        #region Drawing

        /// <summary>
        /// Transforms the children elements
        /// </summary>
        private void TransformChildren()
        {
            // Find the number of items
            int childrenCount = this.childrenIndex.Count;

            // Loop through all the children
            foreach (var dictionaryItem in this.childrenIndex)
            {
                // Find the ID and element
                var id = dictionaryItem.Key;
                var childElement = dictionaryItem.Value;

                // Transform the child
                this.TransformChild((double)id / childrenCount, childElement);
            }
        }

        /// <summary>
        /// Transforms the child element, applies all the carousel behavior transformations to the child
        /// </summary>
        /// <param name="childElement"></param>
        private void TransformChild(double positionDelta, UIElement childElement)
        {
            // Find the center of the canvas
            var targetX = this.Width / 2;
            var targetY = this.Height / 2;

            // Subtract the width of the child
            targetX -= (double)childElement.GetValue(Canvas.WidthProperty) / 2;
            targetY -= (double)childElement.GetValue(Canvas.HeightProperty) / 2;

            // Calculate the full angle for this child
            var angle = fullCircle * positionDelta + quarterCircle - ((rotationOffset + segmentOffset) * segmentCircle);

            // Add the ellipse offset
            if (this.Orientation == Orientation.Vertical)
            {
                targetX += Math.Sin(angle) * this.currentRadiusY;
                targetY += Math.Cos(angle) * this.currentRadiusX;
            }
            else
            {
                targetX += Math.Cos(angle) * this.currentRadiusX;
                targetY += Math.Sin(angle) * this.currentRadiusY;
            }

            // Measure the scale
            var scale = this.MinScale + Math.Sin(angle) / 4;
            var opacity = 1 + this.MinOpacity + Math.Sin(angle);

            // 
            TransformGroup elementTransforms = new TransformGroup();

            // Set the transform on the child
            elementTransforms.Children.Add(new ScaleTransform()
            {
                ScaleX = scale,
                ScaleY = scale
            });

            // Set the transform on the child
            elementTransforms.Children.Add(new TranslateTransform()
            {
                X = targetX,
                Y = targetY,
            });

            // Apply transforms
            childElement.RenderTransform = elementTransforms;

            // Set the opacity
            childElement.Opacity = opacity;

            // Set the zIndex
            childElement.SetValue(Canvas.ZIndexProperty, (int)(scale * 1000));
        }

        #endregion 

        #region Animation

        /// <summary>
        /// This event handler is called every frame by the Silverlight application. It's used
        /// to do all of the framebased animations for the Carousel control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            // Animate the selecting of a child
            AnimateChildSelection();

            // Animate the static rotation 
            AnimateRotation();

            // Animatie the radius smoothly
            AnimateRadius();

            // Update the children
            this.TransformChildren();
        }

        /// <summary>
        /// Animates the Radius properties using the transition delay.
        /// </summary>
        private void AnimateRadius()
        {
            // Perform easing on both X and Y properties
            this.RadiusX.EaseToTarget(ref this.currentRadiusX, this.TransitionDelay);
            this.RadiusY.EaseToTarget(ref this.currentRadiusY, this.TransitionDelay);
        }

        /// <summary>
        /// Animates the Rotation property using the transition delay.
        /// </summary>
        private void AnimateRotation()
        {
            // Check if we are are rotating and not at pause
            if (this.rotationEnabled && !this.rotationPaused)
            {
                // Ease to the target rotation instead of applying it directly
                this.RotationSpeed.EaseToTarget(ref this.currentRotationSpeed, this.TransitionDelay);
            }
            // If we aren't rotating smooth rotation to zero
            else if (currentRotationSpeed != 0)
            {
                (0D).EaseToTarget(ref this.currentRotationSpeed, this.TransitionDelay);
            }

            // Add the currentRotationSpeed to the rotation offset
            this.rotationOffset += currentRotationSpeed;

            // Check if the rotation offset is larger than a full cicle, then restart the offset
            if (Math.Abs(this.rotationOffset) > this.childrenIndex.Count)
                this.rotationOffset = this.rotationOffset % this.childrenIndex.Count;
        }

        /// <summary>
        /// Animates the Selection of the children.
        /// </summary>
        private void AnimateChildSelection()
        {
            // Check if we need to apply elasticity
            if (this.Elasticity > 0 && this.Decelleration > 0)
            {
                // Calculate the delta for the bounce
                segmentDelta = (this.segmentOffsetTarget - this.segmentOffset) * this.Decelleration + segmentDelta * this.Elasticity;

                // Add a portion of the difference to the offset
                this.segmentOffset += segmentDelta / this.TransitionDelay;
            }
            else
            {
                // If we aren't using bounce smooth segment offset to target
                this.segmentOffsetTarget.EaseToTarget(ref this.segmentOffset, this.TransitionDelay);
            }
        }

        #endregion

        /// <summary>
        /// Selects the item specified and shows it first in the carousel orientation
        /// </summary>
        /// <param name="selectedControl"></param>
        public void SelectItem(UIElement uIElement, bool animate)
        {
            // Select the dictionary element that contains the ID using LINQ
            int id = (from index in this.childrenIndex
                      where index.Value.Equals(uIElement)
                      select index.Key).Single();

            // Call the selecting event
            if (this.SelectingItem != null)
                this.SelectingItem(this, new CarouselItemEventArgs()
                {
                    OriginalSource = uIElement
                });

            // Call the deselected event on the previous item
            if (this.DeselectedItem != null)
                this.DeselectedItem(this, new CarouselItemEventArgs()
                {
                    OriginalSource = this.childrenIndex[this.currentItem]
                });

            // Check if we have a delay, indicating we need to animate 
            if (this.TransitionDelay > 0 && animate)
            {
                // Set the target
                this.segmentOffsetTarget = id;

                // Calculate the distance from each direction
                var distanceClockwise = (this.segmentOffset - this.segmentOffsetTarget) + this.rotationOffset;
                var distanceCounterwise = this.childrenIndex.Count - Math.Abs(distanceClockwise);

                // If the clockwise distance is faster, add it to the offset
                if (Math.Abs(distanceClockwise) < Math.Abs(distanceCounterwise))
                    segmentOffset = segmentOffsetTarget + distanceClockwise;

                // If the counterclockwise distance is faster, add that 
                if (Math.Abs(distanceCounterwise) < Math.Abs(distanceClockwise))
                {
                    // If we are switching orientation, subtract  the distance 
                    if (segmentOffset > segmentOffsetTarget)
                        distanceCounterwise *= -1;

                    // Add it to the offset
                    segmentOffset = segmentOffsetTarget + distanceCounterwise;
                }
            }
            else // Set the offset directly without an animation
            {
                // Update the segmentOffset
                this.segmentOffset = id;

                // Update the transformations
                this.TransformChildren();
            }

            // Check for rotation 
            if (this.rotationOffset != 0)
            {
                // Reset the rotation because it has been added to the segmentOffset
                this.rotationOffset = 0;
                this.currentRotationSpeed = 0;

                // Always pause the rotation after a selection
                this.rotationPaused = true;
            }

            // Set the current item for future reference
            this.currentItem = id;
        }

        /// <summary>
        /// Selects the next item in the carousel
        /// </summary>
        public void Next()
        {
            // Find next id
            int next = this.currentItem + 1;

            // Check if the id exists
            if (next >= this.childrenIndex.Count)
                next = 0;

            // Select the item
            this.SelectItem(this.childrenIndex[next], true);
        }

        /// <summary>
        /// Selects the previous item in the carousel
        /// </summary>
        public void Previous()
        {
            // Find next id
            int previous = this.currentItem - 1;

            // Check if the id exists
            if (previous < 0)
                previous = this.childrenIndex.Count - 1;

            // Select the item
            this.SelectItem(this.childrenIndex[previous], true);
        }

        /// <summary>
        /// Enables or disabled the rotation of the carousel
        /// </summary>
        /// <param name="enable"></param>
        public void EnableRotation(bool enable)
        {
            // Handle zero speed
            if (this.RotationSpeed == 0)
                throw new ArgumentException("RotationSpeed must have a value other than zero.");

            // Change the rotation 
            this.rotationEnabled = enable;
            this.rotationPaused = !enable;
        }

        /// <summary>
        /// Reverses the rotation
        /// </summary>
        public void ReverseRotation()
        {
            // Simply negate the speed
            this.RotationSpeed *= -1;
        }

        /// <summary>
        /// Toggles the rotation pause state
        /// </summary>
        public void RotationPauseToggle()
        {
            // Toggle the pause state
            this.rotationPaused = !this.rotationPaused;
        }

        /// <summary>
        /// Opens the carousel from a closed state.
        /// </summary>
        public void Open()
        {
            // Reset the current radius
            this.currentRadiusX = 0;
            this.currentRadiusY = 0;
        }
    }
}
