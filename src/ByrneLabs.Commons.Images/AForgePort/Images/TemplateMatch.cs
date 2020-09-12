// AForge Image Processing Library
// AForge.NET framework
// http://www.aforgenet.com/framework/
//
// Copyright © Andrew Kirillov, 2005-2009
// andrew.kirillov@aforgenet.com
//

using System.Drawing;

namespace ByrneLabs.Commons.Images.AForgePort.Images
{
    /// <summary>
    /// Template match class keeps information about found template match. The class is
    /// used with template matching algorithms implementing <see cref="ITemplateMatching"/>
    /// interface.
    /// </summary>
    public class TemplateMatch
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateMatch"/> class.
        /// </summary>
        /// 
        /// <param name="rect">Rectangle of the matching area.</param>
        /// <param name="similarity">Similarity between template and found matching, [0..1].</param>
        /// 
        public TemplateMatch(Rectangle rect, float similarity)
        {
            Rectangle = rect;
            Similarity = similarity;
        }

        /// <summary>
        /// Rectangle of the matching area.
        /// </summary>
        public Rectangle Rectangle { get; }
        /// <summary>
        /// Similarity between template and found matching, [0..1].
        /// </summary>
        public float Similarity { get; }
    }
}
