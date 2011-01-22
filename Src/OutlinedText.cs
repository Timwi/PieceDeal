using System;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

namespace PieceDeal
{
    public class OutlinedText : FrameworkElement
    {
        private Geometry _textGeometry;

        /// <summary>
        /// Invoked when a dependency property has changed. Generate a new FormattedText object to display.
        /// </summary>
        /// <param name="d">OutlineText object whose property was updated.</param>
        /// <param name="e">Event arguments for the dependency property.</param>
        private static void OnOutlineTextInvalidated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((OutlinedText) d).updateGeometry();
        }


        /// <summary>
        /// OnRender override draws the geometry of the text.
        /// </summary>
        /// <param name="drawingContext">Drawing context of the OutlineText control.</param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            updateGeometry();
            // DrawGeometry() by default draws the fill first, then the pen. But we want the fill to be "complete" and the outline to be around it.
            drawingContext.DrawGeometry(null, Stroke, _textGeometry);
            drawingContext.DrawGeometry(Fill, null, _textGeometry);
        }

        /// <summary>Create the outline geometry based on the formatted text.</summary>
        private void updateGeometry()
        {
            FontStyle fontStyle = FontStyles.Normal;
            FontWeight fontWeight = FontWeights.Medium;

            if (Bold == true) fontWeight = FontWeights.Bold;
            if (Italic == true) fontStyle = FontStyles.Italic;

            // Create the formatted text based on the properties set.
            FormattedText formattedText = new FormattedText(
                Text,
                CultureInfo.GetCultureInfo("en-us"),
                FlowDirection.LeftToRight,
                new Typeface(Font, fontStyle, fontWeight, FontStretches.Normal),
                FontSize,
                Brushes.Black // This brush does not matter since we use the geometry of the text. 
            );

            // Build the geometry object that represents the text.
            _textGeometry = formattedText.BuildGeometry(new Point(0, 0));

            // set the size of the custom control based on the size of the text
            this.MinWidth = formattedText.Width;
            this.MinHeight = formattedText.Height;
        }

        /// <summary>
        /// Specifies whether the font should display Bold font weight.
        /// </summary>
        public bool Bold
        {
            get { return (bool) GetValue(BoldProperty); }
            set { SetValue(BoldProperty, value); }
        }

        /// <summary>
        /// Identifies the Bold dependency property.
        /// </summary>
        public static readonly DependencyProperty BoldProperty = DependencyProperty.Register(
            "Bold",
            typeof(bool),
            typeof(OutlinedText),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.AffectsRender,
                new PropertyChangedCallback(OnOutlineTextInvalidated),
                null
            )
        );

        /// <summary>
        /// Specifies the brush to use for the fill of the formatted text.
        /// </summary>
        public Brush Fill
        {
            get { return (Brush) GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        /// <summary>
        /// Identifies the Fill dependency property.
        /// </summary>
        public static readonly DependencyProperty FillProperty = DependencyProperty.Register(
            "Fill",
            typeof(Brush),
            typeof(OutlinedText),
            new FrameworkPropertyMetadata(
                new SolidColorBrush(Colors.LightSteelBlue),
                FrameworkPropertyMetadataOptions.AffectsRender,
                new PropertyChangedCallback(OnOutlineTextInvalidated),
                null
            )
        );

        /// <summary>
        /// The font to use for the displayed formatted text.
        /// </summary>
        public FontFamily Font
        {
            get { return (FontFamily) GetValue(FontProperty); }
            set { SetValue(FontProperty, value); }
        }

        /// <summary>
        /// Identifies the Font dependency property.
        /// </summary>
        public static readonly DependencyProperty FontProperty = DependencyProperty.Register(
            "Font",
            typeof(FontFamily),
            typeof(OutlinedText),
            new FrameworkPropertyMetadata(
                new FontFamily("Arial"),
                FrameworkPropertyMetadataOptions.AffectsRender,
                new PropertyChangedCallback(OnOutlineTextInvalidated),
                null
            )
        );

        /// <summary>
        /// The current font size.
        /// </summary>
        public double FontSize
        {
            get { return (double) GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        /// <summary>
        /// Identifies the FontSize dependency property.
        /// </summary>
        public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register(
            "FontSize",
            typeof(double),
            typeof(OutlinedText),
            new FrameworkPropertyMetadata(
                 (double) 48.0,
                 FrameworkPropertyMetadataOptions.AffectsRender,
                 new PropertyChangedCallback(OnOutlineTextInvalidated),
                 null
             )
        );

        /// <summary>
        /// Specifies whether the font should display Italic font style.
        /// </summary>
        public bool Italic
        {
            get { return (bool) GetValue(ItalicProperty); }
            set { SetValue(ItalicProperty, value); }
        }

        /// <summary>
        /// Identifies the Italic dependency property.
        /// </summary>
        public static readonly DependencyProperty ItalicProperty = DependencyProperty.Register(
            "Italic",
            typeof(bool),
            typeof(OutlinedText),
            new FrameworkPropertyMetadata(
                 false,
                 FrameworkPropertyMetadataOptions.AffectsRender,
                 new PropertyChangedCallback(OnOutlineTextInvalidated),
                 null
             )
        );

        /// <summary>
        /// Specifies the brush to use for the stroke and optional highlight of the formatted text.
        /// </summary>
        public Pen Stroke
        {
            get { return (Pen) GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }

        /// <summary>
        /// Identifies the Stroke dependency property.
        /// </summary>
        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(
            "Stroke",
            typeof(Pen),
            typeof(OutlinedText),
            new FrameworkPropertyMetadata(
                 new Pen(new SolidColorBrush(Colors.Black), 2),
                 FrameworkPropertyMetadataOptions.AffectsRender,
                 new PropertyChangedCallback(OnOutlineTextInvalidated),
                 null
             )
        );

        /// <summary>
        /// Specifies the text string to display.
        /// </summary>
        public string Text
        {
            get { return (string) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        /// <summary>
        /// Identifies the Text dependency property.
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text",
            typeof(string),
            typeof(OutlinedText),
            new FrameworkPropertyMetadata(
                 "",
                 FrameworkPropertyMetadataOptions.AffectsRender,
                 new PropertyChangedCallback(OnOutlineTextInvalidated),
                 null
             )
        );
    }
}
