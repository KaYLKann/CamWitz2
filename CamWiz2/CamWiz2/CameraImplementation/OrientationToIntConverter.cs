using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace CamWiz2.CameraImplementation
{
    public class OrientationToIntConverter : IValueConverter
    {
        #region Public Methods

        /// <summary>
        /// Convert the specified value, targetType, parameter and culture.
        /// </summary>
        /// <param name="value">Value.</param>
        /// <param name="targetType">Target type.</param>
        /// <param name="parameter">Parameter.</param>
        /// <param name="culture">Culture.</param>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                var str = parameter as string;

                if (str != null)
                {
                    // split string by ',', convert to int and store in case list
                    var cases = str.Split(',').Select(x => Int32.Parse(x)).ToList();

                    if (value is Orientation)
                    {
                        switch ((Orientation)value)
                        {
                            case Orientation.LandscapeRight:
                            case Orientation.LandscapeLeft:
                                return cases[0];
                            case Orientation.Portrait:
                                return cases[1];
                            case Orientation.None:
                                return cases[0];
                        }
                    }
                }
            }
            catch (Exception error)
            {
                
            }

            return 0;
        }

        /// <summary>
        /// Converts the back.
        /// </summary>
        /// <returns>The back.</returns>
        /// <param name="value">Value.</param>
        /// <param name="targetType">Target type.</param>
        /// <param name="parameter">Parameter.</param>
        /// <param name="culture">Culture.</param>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
