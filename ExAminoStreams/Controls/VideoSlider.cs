using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace ExAminoStreams.Controls
{
    [ToolboxBitmap(typeof(TrackBar))]
    [DefaultEvent("Scroll"), DefaultProperty("BarInnerColor")]
    public partial class VideoSlider : Control
    {
        #region Events

        /// <summary>
        /// Fires when Slider position has changed
        /// </summary>
        [Description("Event fires when the Value property changes")]
        [Category("Action")]
        public event EventHandler ValueChanged;

        /// <summary>
        /// Fires when user scrolls the Slider
        /// </summary>
        [Description("Event fires when the Slider position is changed")]
        [Category("Behavior")]
        public event ScrollEventHandler Scroll;

        #endregion Events

        #region Properties

        private Rectangle barRect; //bounding rectangle of bar area
        private Rectangle elapsedRect; //bounding rectangle of elapsed area

        // Margin left & right (bottom & Top)
        private int OffsetL = 0;

        private int OffsetR = 0;

        #region Thumb

        private Rectangle thumbRect; //bounding rectangle of thumb area

        /// <summary>
        /// Gets the thumb rect. Usefull to determine bounding rectangle when creating custom thumb shape.
        /// </summary>
        /// <value>The thumb rect.</value>
        [Browsable(false)]
        public Rectangle ThumbRect
        {
            get { return thumbRect; }
        }

        private Size _thumbSize = new Size(16, 16);

        /// <summary>
        /// Gets or sets the size of the thumb.
        /// </summary>
        /// <value>The size of the thumb.</value>
        /// <exception cref="T:System.ArgumentOutOfRangeException">exception thrown when value is lower than zero or grather than half of appropiate dimension</exception>
        [Description("Set Slider thumb size")]
        [Category(nameof(VideoSlider))]
        [DefaultValue(16)]
        public Size ThumbSize
        {
            get { return _thumbSize; }
            set
            {
                int h = value.Height;
                int w = value.Width;
                if (h > 0 && w > 0)
                    _thumbSize = new Size(w, h);
                else
                    throw new ArgumentOutOfRangeException("TrackSize has to be greather than zero and lower than half of Slider width");

                Invalidate();
            }
        }

        private Size _thumbRoundRectSize = new Size(16, 16);

        /// <summary>
        /// Gets or sets the size of the thumb round rectangle edges.
        /// </summary>
        /// <value>The size of the thumb round rectangle edges.</value>
        [Description("Set Slider's thumb round rect size")]
        [Category(nameof(VideoSlider))]
        [DefaultValue(typeof(Size), "16; 16")]
        public Size ThumbRoundRectSize
        {
            get { return _thumbRoundRectSize; }
            set
            {
                int h = value.Height, w = value.Width;
                if (h <= 0) h = 1;
                if (w <= 0) w = 1;
                _thumbRoundRectSize = new Size(w, h);
                Invalidate();
            }
        }

        #endregion Thumb

        #region Appearance

        private int _padding = 0;

        /// <summary>
        /// Gets or sets the padding (inside margins: left & right or bottom & top)
        /// </summary>
        /// <value>The padding.</value>
        [Description("Set Slider padding (inside margins: left & right or bottom & top)")]
        [Category(nameof(VideoSlider))]
        [DefaultValue(0)]
        public new int Padding
        {
            get { return _padding; }
            set
            {
                if (_padding != value)
                {
                    _padding = value;
                    OffsetL = OffsetR = _padding;

                    Invalidate();
                }
            }
        }

        #endregion Appearance

        #region Values

        private long _trackerValue = 30;

        /// <summary>
        /// Gets or sets the value of Slider.
        /// </summary>
        /// <value>The value.</value>
        /// <exception cref="T:System.ArgumentOutOfRangeException">exception thrown when value is outside appropriate range (min, max)</exception>
        [Description("Set Slider value")]
        [Category(nameof(VideoSlider))]
        [DefaultValue(30)]
        public long Value
        {
            get { return _trackerValue; }
            set
            {
                if (value >= _minimum & value <= _maximum)
                {
                    _trackerValue = value;
                    ValueChanged?.Invoke(this, new EventArgs());
                    Invalidate();
                }
                else throw new ArgumentOutOfRangeException("Value is outside appropriate range (min, max)");
            }
        }

        private long _minimum = 0;

        /// <summary>
        /// Gets or sets the minimum value.
        /// </summary>
        /// <value>The minimum value.</value>
        /// <exception cref="T:System.ArgumentOutOfRangeException">exception thrown when minimal value is greater than maximal one</exception>
        [Description("Set Slider minimal point")]
        [Category(nameof(VideoSlider))]
        [DefaultValue(0)]
        public long Minimum
        {
            get { return _minimum; }
            set
            {
                if (value < _maximum)
                {
                    _minimum = value;
                    if (_trackerValue < _minimum)
                    {
                        _trackerValue = _minimum;
                        ValueChanged?.Invoke(this, new EventArgs());
                    }
                    Invalidate();
                }
                else throw new ArgumentOutOfRangeException("Minimal value is greather than maximal one");
            }
        }

        private long _maximum = 100;

        /// <summary>
        /// Gets or sets the maximum value.
        /// </summary>
        /// <value>The maximum value.</value>
        /// <exception cref="T:System.ArgumentOutOfRangeException">exception thrown when maximal value is lower than minimal one</exception>
        [Description("Set Slider maximal point")]
        [Category(nameof(VideoSlider))]
        [DefaultValue(100)]
        public long Maximum
        {
            get { return _maximum; }
            set
            {
                if (value > _minimum)
                {
                    _maximum = value;
                    if (_trackerValue > _maximum)
                    {
                        _trackerValue = _maximum;
                        ValueChanged?.Invoke(this, new EventArgs());
                    }
                    Invalidate();
                }
                //else throw new ArgumentOutOfRangeException("Maximal value is lower than minimal one");
            }
        }

        private long _smallChange = 1;

        /// <summary>
        /// Gets or sets trackbar's small change. It affects how to behave when directional keys are pressed
        /// </summary>
        /// <value>The small change value.</value>
        [Description("Set trackbar's small change")]
        [Category(nameof(VideoSlider))]
        [DefaultValue(1)]
        public long SmallChange
        {
            get { return _smallChange; }
            set { _smallChange = value; }
        }

        private long _largeChange = 5;

        /// <summary>
        /// Gets or sets trackbar's large change. It affects how to behave when PageUp/PageDown keys are pressed
        /// </summary>
        /// <value>The large change value.</value>
        [Description("Set trackbar's large change")]
        [Category(nameof(VideoSlider))]
        [DefaultValue(5)]
        public long LargeChange
        {
            get { return _largeChange; }
            set { _largeChange = value; }
        }

        #endregion Values

        #region Colors

        private Color _thumbOuterColor = Color.White;

        /// <summary>
        /// Gets or sets the thumb outer color.
        /// </summary>
        /// <value>The thumb outer color.</value>
        [Description("Sets Slider thumb outer color")]
        [Category(nameof(VideoSlider))]
        [DefaultValue(typeof(Color), "White")]
        public Color ThumbOuterColor
        {
            get { return _thumbOuterColor; }
            set
            {
                _thumbOuterColor = value;
                Invalidate();
            }
        }

        private Color _thumbInnerColor = Color.FromArgb(21, 56, 152);

        /// <summary>
        /// Gets or sets the inner color of the thumb.
        /// </summary>
        /// <value>The inner color of the thumb.</value>
        [Description("Set Slider thumb inner color")]
        [Category(nameof(VideoSlider))]
        public Color ThumbInnerColor
        {
            get { return _thumbInnerColor; }
            set
            {
                _thumbInnerColor = value;
                Invalidate();
            }
        }

        private Color _thumbPenColor = Color.FromArgb(21, 56, 152);

        /// <summary>
        /// Gets or sets the color of the thumb pen.
        /// </summary>
        /// <value>The color of the thumb pen.</value>
        [Description("Set Slider thumb pen color")]
        [Category(nameof(VideoSlider))]
        public Color ThumbPenColor
        {
            get { return _thumbPenColor; }
            set
            {
                _thumbPenColor = value;
                Invalidate();
            }
        }

        private Color _barInnerColor = Color.Black;

        /// <summary>
        /// Gets or sets the inner color of the bar.
        /// </summary>
        /// <value>The inner color of the bar.</value>
        [Description("Set Slider bar inner color")]
        [Category(nameof(VideoSlider))]
        [DefaultValue(typeof(Color), "Black")]
        public Color BarInnerColor
        {
            get { return _barInnerColor; }
            set
            {
                _barInnerColor = value;
                Invalidate();
            }
        }

        private Color _elapsedInnerColor = Color.FromArgb(21, 56, 152);

        /// <summary>
        /// Gets or sets the inner color of the elapsed.
        /// </summary>
        /// <value>The inner color of the elapsed.</value>
        [Description("Set Slider's elapsed part inner color")]
        [Category(nameof(VideoSlider))]
        public Color ElapsedInnerColor
        {
            get { return _elapsedInnerColor; }
            set
            {
                _elapsedInnerColor = value;
                Invalidate();
            }
        }

        private Color _tickColor = Color.White;

        /// <summary>
        /// Gets or sets the color of the graduations
        /// </summary>
        [Description("Color of graduations")]
        [Category(nameof(VideoSlider))]
        public Color TickColor
        {
            get { return _tickColor; }
            set
            {
                if (value != _tickColor)
                {
                    _tickColor = value;
                    Invalidate();
                }
            }
        }

        #endregion Colors

        #region Divisions

        // For ex: if values are multiples of 50,
        // values = 0, 50, 100, 150 etc...
        //set TickDivide to 50
        // And ticks will be displayed as
        // values = 0, 1, 2, 3 etc...
        private float _tickDivide = 0;

        [Description("Gets or sets a value used to divide the graduation")]
        [Category(nameof(VideoSlider))]
        public float TickDivide
        {
            get { return _tickDivide; }
            set
            {
                _tickDivide = value;
                Invalidate();
            }
        }

        private float _tickAdd = 0;

        [Description("Gets or sets a value added to the graduation")]
        [Category(nameof(VideoSlider))]
        public float TickAdd
        {
            get { return _tickAdd; }
            set
            {
                _tickAdd = value;
                Invalidate();
            }
        }

        private TickStyle _tickStyle = TickStyle.TopLeft;

        /// <summary>
        /// Gets or sets where to display the ticks (None, both top-left, bottom-right)
        /// </summary>
        [Description("Gets or sets where to display the ticks")]
        [Category(nameof(VideoSlider))]
        [DefaultValue(TickStyle.TopLeft)]
        public TickStyle TickStyle
        {
            get { return _tickStyle; }
            set
            {
                _tickStyle = value;
                Invalidate();
            }
        }

        private decimal _scaleDivisions = 10;

        /// <summary>
        /// How many divisions of maximum?
        /// </summary>
        [Description("Set the number of intervals between minimum and maximum")]
        [Category(nameof(VideoSlider))]
        public decimal ScaleDivisions
        {
            get { return _scaleDivisions; }
            set
            {
                if (value > 0)
                    _scaleDivisions = value;

                Invalidate();
            }
        }

        private decimal _scaleSubDivisions = 5;

        /// <summary>
        /// How many subdivisions for each division
        /// </summary>
        [Description("Set the number of subdivisions between main divisions of graduation.")]
        [Category(nameof(VideoSlider))]
        public decimal ScaleSubDivisions
        {
            get { return _scaleSubDivisions; }
            set
            {
                if (value > 0 && _scaleDivisions > 0 && (_maximum - _minimum) / ((value + 1) * _scaleDivisions) > 0)
                    _scaleSubDivisions = value;

                Invalidate();
            }
        }

        private bool _showSmallScale = false;

        /// <summary>
        /// Shows Small Scale marking.
        /// </summary>
        [Description("Show or hide subdivisions of graduations")]
        [Category(nameof(VideoSlider))]
        public bool ShowSmallScale
        {
            get { return _showSmallScale; }
            set
            {
                if (value == true)
                {
                    if (_scaleDivisions > 0 && _scaleSubDivisions > 0 && (_maximum - _minimum) / ((_scaleSubDivisions + 1) * _scaleDivisions) > 0)
                    {
                        _showSmallScale = value;
                        Invalidate();
                    }
                    else
                        _showSmallScale = false;
                }
                else
                {
                    _showSmallScale = value;
                    // need to redraw
                    Invalidate();
                }
            }
        }

        private bool _showBigScale = false;

        /// <summary>
        /// Shows Small Scale marking.
        /// </summary>
        [Description("Show or hide subdivisions of graduations")]
        [Category(nameof(VideoSlider))]
        public bool ShowBigScale
        {
            get { return _showBigScale; }
            set
            {
                if (value == true)
                {
                    if (_scaleDivisions > 0 && _scaleSubDivisions > 0 && (_maximum - _minimum) / ((_scaleSubDivisions + 1) * _scaleDivisions) > 0)
                    {
                        _showBigScale = value;
                        Invalidate();
                    }
                    else
                        _showBigScale = false;
                }
                else
                {
                    _showBigScale = value;
                    // need to redraw
                    Invalidate();
                }
            }
        }

        private bool _showDivisionsText = true;

        /// <summary>
        /// Shows Small Scale marking.
        /// </summary>
        [Description("Show or hide text value of graduations")]
        [Category(nameof(VideoSlider))]
        public bool ShowDivisionsText
        {
            get { return _showDivisionsText; }
            set
            {
                _showDivisionsText = value;
                Invalidate();
            }
        }

        #endregion Divisions

        #region Font

        /// <summary>
        /// Get or Sets the Font of the Text being displayed.
        /// </summary>
        [Bindable(true),
        Browsable(true),
        Category(nameof(VideoSlider)),
        Description("Get or Sets the Font of the Text being displayed."),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Visible),
        EditorBrowsable(EditorBrowsableState.Always)]
        public override Font Font
        {
            get
            {
                return base.Font;
            }
            set
            {
                base.Font = value;
                Invalidate();
                OnFontChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Get or Sets the Font of the Text being displayed.
        /// </summary>
        [Bindable(true),
        Browsable(true),
        Category(nameof(VideoSlider)),
        Description("Get or Sets the Color of the Text being displayed."),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Visible),
        EditorBrowsable(EditorBrowsableState.Always)]
        public override Color ForeColor
        {
            get
            {
                return base.ForeColor;
            }
            set
            {
                base.ForeColor = value;
                Invalidate();
                OnForeColorChanged(EventArgs.Empty);
            }
        }

        #endregion Font

        #endregion Properties

        #region Constructors

        public VideoSlider() : this(0, 100, 30)
        {
        }

        // <summary>
        /// Initializes a new instance of the <see cref="ColorSlider"/> class.
        /// </summary>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="value">The current value.</param>
        public VideoSlider(long min, long max, long value)
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw | ControlStyles.Selectable |
                     ControlStyles.SupportsTransparentBackColor | ControlStyles.UserMouse |
                     ControlStyles.UserPaint, true);

            // Default backcolor
            BackColor = Color.FromArgb(70, 77, 95);
            ForeColor = Color.White;

            // Font
            this.Font = new Font("Microsoft Sans Serif", 6f);

            Minimum = min;
            Maximum = max;
            Value = value;
        }

        #endregion Constructors

        #region Paint

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Paint"></see> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs"></see> that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            if (!Enabled)
            {
                Color[] desaturatedColors = DesaturateColors(_thumbOuterColor,
                                                             _thumbInnerColor,
                                                             _thumbPenColor,
                                                             _barInnerColor,
                                                             _elapsedInnerColor);
                DrawColorSlider(paintArgs: e,
                                thumbOuterColorPaint: desaturatedColors[0],
                                thumbInnerColorPaint: desaturatedColors[1],
                                thumbPenColorPaint: desaturatedColors[2],
                                barInnerColorPaint: desaturatedColors[3],
                                elapsedInnerColorPaint: desaturatedColors[4]);
            }
            else
            {
                DrawColorSlider(paintArgs: e,
                                thumbOuterColorPaint: _thumbOuterColor,
                                thumbInnerColorPaint: _thumbInnerColor,
                                thumbPenColorPaint: _thumbPenColor,
                                barInnerColorPaint: _barInnerColor,
                                elapsedInnerColorPaint: _elapsedInnerColor);
            }
        }

        /// <summary>
        /// Draws the colorslider control using passed colors.
        /// </summary>
        /// <param name="paintArgs">The <see cref="T:System.Windows.Forms.PaintEventArgs"/> instance containing the event data.</param>
        /// <param name="thumbOuterColorPaint">The thumb outer color paint.</param>
        /// <param name="thumbInnerColorPaint">The thumb inner color paint.</param>
        /// <param name="thumbPenColorPaint">The thumb pen color paint.</param>
        /// <param name="barInnerColorPaint">The bar inner color paint.</param>
        /// <param name="barPenColorPaint">The bar pen color paint.</param>
        /// <param name="elapsedInnerColorPaint">The elapsed inner color paint.</param>
        private void DrawColorSlider(PaintEventArgs paintArgs,
                                     Color thumbOuterColorPaint,
                                     Color thumbInnerColorPaint,
                                     Color thumbPenColorPaint,
                                     Color barInnerColorPaint,
                                     Color elapsedInnerColorPaint)
        {
            try
            {
                //adjust drawing rects
                barRect = ClientRectangle;

                //set up thumbRect
                decimal TrackX = OffsetL + ((_trackerValue - _minimum) * (ClientRectangle.Width - OffsetL - OffsetR + (_thumbSize.Width/2))) / (_maximum - _minimum);
                thumbRect = new Rectangle((int)TrackX, barRect.Y + ClientRectangle.Height / 2 - _thumbSize.Height / 2, _thumbSize.Width, _thumbSize.Height);

                LinearGradientMode gradientOrientation;

                barRect.X = barRect.X + OffsetL;
                barRect.Width = barRect.Width - OffsetL - OffsetR;

                gradientOrientation = LinearGradientMode.Vertical;

                elapsedRect = barRect;
                elapsedRect.Width = (thumbRect.Left + _thumbSize.Width / 2) - OffsetL;

                //get thumb shape path
                GraphicsPath thumbPath = CreateRoundRectPath(thumbRect, _thumbRoundRectSize);

                //draw bar

                #region draw inner bar

                // inner bar is a single line
                // draw the line on the whole length of the control
                paintArgs.Graphics.DrawLine(new Pen(barInnerColorPaint, 3f), barRect.X, barRect.Y + barRect.Height / 2, barRect.X + barRect.Width, barRect.Y + barRect.Height / 2);

                #endregion draw inner bar

                #region draw elapsed bar

                //draw elapsed inner bar (single line too)
                paintArgs.Graphics.DrawLine(new Pen(elapsedInnerColorPaint, 3f), barRect.X, barRect.Y + barRect.Height / 2, barRect.X + elapsedRect.Width, barRect.Y + barRect.Height / 2);

                #endregion draw elapsed bar

                #region draw thumb

                //draw thumb
                Color newthumbOuterColorPaint = thumbOuterColorPaint, newthumbInnerColorPaint = thumbInnerColorPaint;
                LinearGradientBrush lgbThumb;
                lgbThumb = new LinearGradientBrush(thumbRect, newthumbOuterColorPaint, newthumbInnerColorPaint, gradientOrientation);

                using (lgbThumb)
                {
                    lgbThumb.WrapMode = WrapMode.TileFlipXY;

                    paintArgs.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    paintArgs.Graphics.FillPath(lgbThumb, thumbPath);

                    //draw thumb band
                    Color newThumbPenColor = thumbPenColorPaint;

                    using (Pen thumbPen = new Pen(newThumbPenColor))
                    {
                        paintArgs.Graphics.DrawPath(thumbPen, thumbPath);
                    }
                }

                #endregion draw thumb

                #region draw ticks

                // Draw the ticks (main divisions, subdivisions and text)
                if (_tickStyle != TickStyle.None)
                {
                    int x1, x2, y1, y2 = 0;
                    int nbticks = 1 + (int)(_scaleDivisions * (_scaleSubDivisions + 1));
                    int interval = 0;
                    int start = 0;
                    int W = 0;
                    float rulerValue = 0;
                    int offset = 0;

                    // Calculate width W to draw graduations
                    // Remove the width of the thumb (half thumb at each end)
                    // in order that when the thumb is at minimum position or maximum position,
                    // the graduation coincide with the middle of the thumb
                    start = thumbRect.Width / 2;
                    W = barRect.Width - thumbRect.Width;
                    rulerValue = (float)_minimum;
                    offset = 2 + thumbRect.Height / 2;

                    // pen for ticks
                    Pen penTickL = new(_tickColor, 1f);
                    Pen penTickS = new(_tickColor, 1f);
                    int idx = 0;
                    int scaleL = 5;     // division length
                    int scaleS = 3;     // subdivision length

                    // strings graduations
                    float tx = 0;
                    float ty = 0;
                    int startDiv = 0;

                    Color _scaleColor = ForeColor;
                    SolidBrush br = new(_scaleColor);

                    // Calculate max size of text
                    string str = String.Format("{0,0:##}", _maximum);
                    Font font = this.Font;
                    SizeF maxsize = paintArgs.Graphics.MeasureString(str, font);

                    for (int i = 0; i <= _scaleDivisions; i++)
                    {
                        // Calculate current text size
                        float val = rulerValue;

                        // apply a transformation to the ticks displayed
                        if (_tickDivide != 0)
                            val /= _tickDivide;

                        if (_tickAdd != 0)
                            val += _tickAdd;

                        str = String.Format("{0:0.##}", val);
                        SizeF size = paintArgs.Graphics.MeasureString(str, font);

                        // Draw string graduations
                        if (_showDivisionsText)
                        {
                            if (_tickStyle == TickStyle.TopLeft || _tickStyle == TickStyle.Both)
                            {
                                tx = (start + barRect.X + interval) - (float)(size.Width * 0.5);
                                ty = barRect.Y + barRect.Height / 2 - size.Height - scaleL - offset;
                                paintArgs.Graphics.DrawString(str, font, br, tx, ty);
                            }
                            if (_tickStyle == TickStyle.BottomRight || _tickStyle == TickStyle.Both)
                            {
                                tx = (start + barRect.X + interval) - (float)(size.Width * 0.5);
                                ty = barRect.Y + barRect.Height / 2 + scaleL + offset;
                                paintArgs.Graphics.DrawString(str, font, br, tx, ty);
                            }
                        }

                        // draw main ticks
                        if (_tickStyle == TickStyle.TopLeft || _tickStyle == TickStyle.Both)
                        {
                            x1 = start + barRect.X + interval;
                            y1 = barRect.Y + barRect.Height / 2 - scaleL - offset;
                            x2 = start + barRect.X + interval;
                            y2 = barRect.Y + barRect.Height / 2 - offset;
                            paintArgs.Graphics.DrawLine(penTickL, x1, y1, x2, y2);
                        }
                        if (_tickStyle == TickStyle.BottomRight || _tickStyle == TickStyle.Both)
                        {
                            x1 = start + barRect.X + interval;
                            y1 = barRect.Y + barRect.Height / 2 + offset;
                            x2 = start + barRect.X + interval;
                            y2 = barRect.Y + barRect.Height / 2 + scaleL + offset;
                            paintArgs.Graphics.DrawLine(penTickL, x1, y1, x2, y2);
                        }

                        rulerValue += (float)((_maximum - _minimum) / (_scaleDivisions));

                        // Draw subdivisions
                        if (i < _scaleDivisions)
                        {
                            for (int j = 0; j <= _scaleSubDivisions; j++)
                            {
                                idx++;
                                interval = idx * W / (nbticks - 1);

                                if (_showSmallScale)
                                {
                                    // Horizontal
                                    if (_tickStyle == TickStyle.TopLeft || _tickStyle == TickStyle.Both)
                                    {
                                        x1 = start + barRect.X + interval;
                                        y1 = barRect.Y + barRect.Height / 2 - scaleS - offset;
                                        x2 = start + barRect.X + interval;
                                        y2 = barRect.Y + barRect.Height / 2 - offset;
                                        paintArgs.Graphics.DrawLine(penTickS, x1, y1, x2, y2);
                                    }
                                    if (_tickStyle == TickStyle.BottomRight || _tickStyle == TickStyle.Both)
                                    {
                                        x1 = start + barRect.X + interval;
                                        y1 = barRect.Y + barRect.Height / 2 + offset;
                                        x2 = start + barRect.X + interval;
                                        y2 = barRect.Y + barRect.Height / 2 + scaleS + offset;
                                        paintArgs.Graphics.DrawLine(penTickS, x1, y1, x2, y2);
                                    }
                                }
                            }
                        }
                    }
                }

                #endregion draw ticks
            }
            catch (Exception Err)
            {
                Console.WriteLine("DrawBackGround Error in " + Name + ":" + Err.Message);
            }
            finally
            {
            }
        }

        #endregion Paint



        #region Overrided events

        private bool mouseInRegion = false;

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.EnabledChanged"></see> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Invalidate();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseEnter"></see> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            mouseInRegion = true;
            Invalidate();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseLeave"></see> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            mouseInRegion = false;
            mouseInThumbRegion = false;
            Invalidate();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseDown"></see> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data.</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                Capture = true;
                Scroll?.Invoke(this, new ScrollEventArgs(ScrollEventType.ThumbTrack, (int)_trackerValue));
                ValueChanged?.Invoke(this, new EventArgs());
                OnMouseMove(e);
            }
        }

        private bool mouseInThumbRegion = false;

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseMove"></see> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            mouseInThumbRegion = IsPointInRect(e.Location, thumbRect);

            if (Capture & e.Button == MouseButtons.Left)
            {
                ScrollEventType set = ScrollEventType.ThumbPosition;
                Point pt = e.Location;
                int p = pt.X;

                int margin = _thumbSize.Height >> 1;
                p -= margin;

                _trackerValue = _minimum + (p - OffsetL) * (_maximum - _minimum) / (ClientRectangle.Width - OffsetL - OffsetR - _thumbSize.Width);

                // Number of divisions
                int nbdiv = (int)(_trackerValue / _smallChange);
                _trackerValue = nbdiv * _smallChange;

                if (_trackerValue <= _minimum)
                {
                    _trackerValue = _minimum;
                    set = ScrollEventType.First;
                }
                else if (_trackerValue >= _maximum)
                {
                    _trackerValue = _maximum;
                    set = ScrollEventType.Last;
                }

                Scroll?.Invoke(this, new ScrollEventArgs(set, (int)_trackerValue));
                ValueChanged?.Invoke(this, new EventArgs());
            }
            Invalidate();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseUp"></see> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data.</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            Capture = false;
            mouseInThumbRegion = IsPointInRect(e.Location, thumbRect);
            Scroll?.Invoke(this, new ScrollEventArgs(ScrollEventType.EndScroll, (int)_trackerValue));
            ValueChanged?.Invoke(this, new EventArgs());
            Invalidate();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.GotFocus"></see> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Invalidate();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.LostFocus"></see> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Invalidate();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.KeyUp"></see> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.KeyEventArgs"></see> that contains the event data.</param>
        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            switch (e.KeyCode)
            {
                case Keys.Down:
                case Keys.Left:
                    SetProperValue(Value - (int)_smallChange);
                    Scroll?.Invoke(this, new ScrollEventArgs(ScrollEventType.SmallDecrement, (int)Value));
                    break;

                case Keys.Up:
                case Keys.Right:
                    SetProperValue(Value + (int)_smallChange);
                    Scroll?.Invoke(this, new ScrollEventArgs(ScrollEventType.SmallIncrement, (int)Value));
                    break;

                case Keys.Home:
                    Value = _minimum;
                    break;

                case Keys.End:
                    Value = _maximum;
                    break;

                case Keys.PageDown:
                    SetProperValue(Value - (int)_largeChange);
                    Scroll?.Invoke(this, new ScrollEventArgs(ScrollEventType.LargeDecrement, (int)Value));
                    break;

                case Keys.PageUp:
                    SetProperValue(Value + (int)_largeChange);
                    Scroll?.Invoke(this, new ScrollEventArgs(ScrollEventType.LargeIncrement, (int)Value));
                    break;
            }
            if (Scroll != null && Value == _minimum) Scroll(this, new ScrollEventArgs(ScrollEventType.First, (int)Value));
            if (Scroll != null && Value == _maximum) Scroll(this, new ScrollEventArgs(ScrollEventType.Last, (int)Value));
            Point pt = PointToClient(Cursor.Position);
            OnMouseMove(new MouseEventArgs(MouseButtons.None, 0, pt.X, pt.Y, 0));
        }

        /// <summary>
        /// Processes a dialog key.
        /// </summary>
        /// <param name="keyData">One of the <see cref="T:System.Windows.Forms.Keys"></see> values that represents the key to process.</param>
        /// <returns>
        /// true if the key was processed by the control; otherwise, false.
        /// </returns>
        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Tab | ModifierKeys == Keys.Shift)
                return base.ProcessDialogKey(keyData);
            else
            {
                OnKeyDown(new KeyEventArgs(keyData));
                return true;
            }
        }

        #endregion Overrided events

        #region Help routines

        /// <summary>
        /// Creates the round rect path.
        /// </summary>
        /// <param name="rect">The rectangle on which graphics path will be spanned.</param>
        /// <param name="size">The size of rounded rectangle edges.</param>
        /// <returns></returns>
        public static GraphicsPath CreateRoundRectPath(Rectangle rect, Size size)
        {
            GraphicsPath gp = new();
            gp.AddLine(rect.Left + size.Width / 2, rect.Top, rect.Right - size.Width / 2, rect.Top);
            gp.AddArc(rect.Right - size.Width, rect.Top, size.Width, size.Height, 270, 90);

            gp.AddLine(rect.Right, rect.Top + size.Height / 2, rect.Right, rect.Bottom - size.Width / 2);
            gp.AddArc(rect.Right - size.Width, rect.Bottom - size.Height, size.Width, size.Height, 0, 90);

            gp.AddLine(rect.Right - size.Width / 2, rect.Bottom, rect.Left + size.Width / 2, rect.Bottom);
            gp.AddArc(rect.Left, rect.Bottom - size.Height, size.Width, size.Height, 90, 90);

            gp.AddLine(rect.Left, rect.Bottom - size.Height / 2, rect.Left, rect.Top + size.Height / 2);
            gp.AddArc(rect.Left, rect.Top, size.Width, size.Height, 180, 90);
            return gp;
        }

        /// <summary>
        /// Desaturates colors from given array.
        /// </summary>
        /// <param name="colorsToDesaturate">The colors to be desaturated.</param>
        /// <returns></returns>
        public static Color[] DesaturateColors(params Color[] colorsToDesaturate)
        {
            Color[] colorsToReturn = new Color[colorsToDesaturate.Length];
            for (int i = 0; i < colorsToDesaturate.Length; i++)
            {
                //use NTSC weighted avarage
                int gray =
                    (int)(colorsToDesaturate[i].R * 0.3 + colorsToDesaturate[i].G * 0.6 + colorsToDesaturate[i].B * 0.1);
                colorsToReturn[i] = Color.FromArgb(-0x010101 * (255 - gray) - 1);
            }
            return colorsToReturn;
        }

        /// <summary>
        /// Lightens colors from given array.
        /// </summary>
        /// <param name="colorsToLighten">The colors to lighten.</param>
        /// <returns></returns>
        public static Color[] LightenColors(params Color[] colorsToLighten)
        {
            Color[] colorsToReturn = new Color[colorsToLighten.Length];
            for (int i = 0; i < colorsToLighten.Length; i++)
                colorsToReturn[i] = ControlPaint.Light(colorsToLighten[i]);

            return colorsToReturn;
        }

        /// <summary>
        /// Sets the trackbar value so that it wont exceed allowed range.
        /// </summary>
        /// <param name="val">The value.</param>
        private void SetProperValue(long val)
        {
            if (val < _minimum) 
                Value = _minimum;
            else if (val > _maximum) 
                Value = _maximum;
            else 
                Value = val;
        }

        /// <summary>
        /// Determines whether rectangle contains given point.
        /// </summary>
        /// <param name="pt">The point to test.</param>
        /// <param name="rect">The base rectangle.</param>
        /// <returns>
        /// 	<c>true</c> if rectangle contains given point; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsPointInRect(Point pt, Rectangle rect)
        {
            return pt.X > rect.Left & pt.X < rect.Right & pt.Y > rect.Top & pt.Y < rect.Bottom;
        }
        #endregion Help routines
    }
}