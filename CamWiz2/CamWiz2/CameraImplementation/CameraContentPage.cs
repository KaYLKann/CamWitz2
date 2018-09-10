using System;
using System.Collections.Generic;
using System.Text;

namespace CamWiz2.CameraImplementation
{
    public class ExtendedContentPage : OrientationPage
    {
        #region Events

        /// <summary>
        /// Occurs when touch handler.
        /// </summary>
        public event EventHandler AlertFinished;

        /// <summary>
        /// Occurs when orientation change.
        /// </summary>
        public event EventHandler<Orientation> OrientationChange;

        #endregion


        /// <summary>
        /// Handles the disappearing.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void HandleDisappearing(object sender, EventArgs e)
        {
            OrientationPage.OrientationHandler -= OrientationPage_OrientationHandler;
        }

        /// <summary>
        /// Trons the page appearing.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void HandleAppearing(object sender, EventArgs e)
        {
            OrientationPage.OrientationHandler -= OrientationPage_OrientationHandler;
        }

        /// <summary>
        /// Orientations the page orientation handler.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        private void OrientationPage_OrientationHandler(object sender, Orientation e)
        {
            PageOrientation = e;

            if (OrientationChange != null)
            {
                OrientationChange(this, e);
            }
        }

        
    }
}
