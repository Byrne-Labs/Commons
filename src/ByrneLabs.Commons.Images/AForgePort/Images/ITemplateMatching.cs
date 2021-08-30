// AForge Image Processing Library
// AForge.NET framework
// http://www.aforgenet.com/framework/
//
// Copyright © Andrew Kirillov, 2005-2009
// andrew.kirillov@aforgenet.com
//

using System.Drawing;
using System.Drawing.Imaging;

namespace ByrneLabs.Commons.Images.AForgePort.Images
{
    /// <summary>
    /// Template matching algorithm's interface.
    /// </summary>
    /// 
    /// <remarks><para>The interface specifies set of methods, which should be implemented by different
    /// unmanagedImageTemplate matching algorithms - algorithms, which search for the given unmanagedImageTemplate in specified
    /// image.</para></remarks>
    /// 
    public interface ITemplateMatching
    {
        /// <summary>
        /// Process image looking for matchings with specified unmanagedImageTemplate.
        /// </summary>
        /// 
        /// <param name="image">Source image to process.</param>
        /// <param name="bitmapTemplate">Template image to search for.</param>
        /// <param name="searchZone">Rectangle in source image to search unmanagedImageTemplate for.</param>
        /// 
        /// <returns>Returns array of found matchings.</returns>
        /// 
        TemplateMatch[] ProcessImage(Bitmap image, Bitmap bitmapTemplate, Rectangle searchZone);

        /// <summary>
        /// Process image looking for matchings with specified unmanagedImageTemplate.
        /// </summary>
        /// 
        /// <param name="imageData">Source image data to process.</param>
        /// <param name="templateData">Template image to search for.</param>
        /// <param name="searchZone">Rectangle in source image to search unmanagedImageTemplate for.</param>
        /// 
        /// <returns>Returns array of found matchings.</returns>
        /// 
        TemplateMatch[] ProcessImage(BitmapData imageData, BitmapData templateData, Rectangle searchZone);

        /// <summary>
        /// Process image looking for matchings with specified template.
        /// </summary>
        /// 
        /// <param name="image">Unmanaged source image to process.</param>
        /// <param name="unmanagedImageTemplate">Unmanaged template image to search for.</param>
        /// <param name="searchZone">Rectangle in source image to search template for.</param>
        /// 
        /// <returns>Returns array of found matchings.</returns>
        /// 
        TemplateMatch[] ProcessImage(UnmanagedImage image, UnmanagedImage unmanagedImageTemplate, Rectangle searchZone);
    }
}
