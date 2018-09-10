using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace CamWiz2.Droid.CameraImplement
{
    [Register("CamWiz2.Android.CameraImplement.AutoFitTextureView")]
    public class AutoFitTextureView : TextureView
    {
        private int mRatioWidth = 0;

        /// <summary>
        /// The height of the m ratio.
        /// </summary>
        private int mRatioHeight = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Camera.Droid.Renderers.CameraView.AutoFitTextureView"/> class.
        /// </summary>
        /// <param name="context">Context.</param>
        public AutoFitTextureView(Context context)
            : this(context, null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Camera.Droid.Renderers.CameraView.AutoFitTextureView"/> class.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <param name="attrs">Attrs.</param>
        public AutoFitTextureView(Context context, IAttributeSet attrs)
            : this(context, attrs, 0)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Camera.Droid.Renderers.CameraView.AutoFitTextureView"/> class.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <param name="attrs">Attrs.</param>
        /// <param name="defStyle">Def style.</param>
        public AutoFitTextureView(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
        {

        }

        /// <summary>
        /// Sets the aspect ratio.
        /// </summary>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        public void SetAspectRatio(int width, int height)
        {
            if (width == 0 || height == 0)
            {
                throw new ArgumentException("Size cannot be negative.");
            }

            mRatioWidth = width;
            mRatioHeight = height;
            RequestLayout();
        }

        /// <summary>
        /// Ons the measure.
        /// </summary>
        /// <param name="widthMeasureSpec">Width measure spec.</param>
        /// <param name="heightMeasureSpec">Height measure spec.</param>
        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);

            int width = MeasureSpec.GetSize(widthMeasureSpec);
            int height = MeasureSpec.GetSize(heightMeasureSpec);

            //SetMeasuredDimension(width, height);

            if (0 == mRatioWidth || 0 == mRatioHeight)
            {
                SetMeasuredDimension(width, height);
            }
            else
            {
                if (width < (float)height * mRatioWidth / (float)mRatioHeight)
                {
                    SetMeasuredDimension(width, width * mRatioHeight / mRatioWidth);
                }
                else
                {
                    SetMeasuredDimension(height * mRatioWidth / mRatioHeight, height);
                }
            }
        }
    }
}