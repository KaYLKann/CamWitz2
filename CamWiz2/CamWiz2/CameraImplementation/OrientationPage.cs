using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace CamWiz2.CameraImplementation
{
    public class OrientationPage : ContentPage
    {
        #region Static Properties

        /// <summary>
        /// The page orientation.
        /// </summary>
        public static Orientation PageOrientation;

        /// <summary>
        /// Occurs when orientation handler.
        /// </summary>
        public static event EventHandler<Orientation> OrientationHandler;

        /// <summary>
        /// Occurs when touch handler.
        /// </summary>
        public static event EventHandler<Point> TouchHandler;

        #endregion

        #region Static Methods

        /// <summary>
        /// Notifies the orientation change.
        /// </summary>
        /// <param name="orientation">Orientation.</param>
        public static void NotifyOrientationChange(Orientation orientation)
        {
            if (OrientationHandler != null)
            {
                OrientationHandler(null, orientation);
            }
        }

        /// <summary>
        /// Notifies the touch.
        /// </summary>
        /// <param name="touchPoint">Touch point.</param>
        public static void NotifyTouch(Point touchPoint)
        {
            if (TouchHandler != null)
            {
                TouchHandler(null, touchPoint);
            }
        }

        #endregion
    }
}
