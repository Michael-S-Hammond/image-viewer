using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ImageViewer.Controls;

public partial class NumericUpDown : UserControl
{
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(int), typeof(NumericUpDown),
            new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(nameof(Minimum), typeof(int), typeof(NumericUpDown),
            new PropertyMetadata(1));

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(int), typeof(NumericUpDown),
            new PropertyMetadata(999));

    public static readonly DependencyProperty IncrementProperty =
        DependencyProperty.Register(nameof(Increment), typeof(int), typeof(NumericUpDown),
            new PropertyMetadata(1));

    private static readonly Regex _numericRegex = new(@"[^0-9]+", RegexOptions.Compiled);

    public NumericUpDown()
    {
        InitializeComponent();
    }

    public int Value
    {
        get => (int)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public int Minimum
    {
        get => (int)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public int Maximum
    {
        get => (int)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public int Increment
    {
        get => (int)GetValue(IncrementProperty);
        set => SetValue(IncrementProperty, value);
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NumericUpDown control)
        {
            control.ValidateValue();
        }
    }

    private void ValidateValue()
    {
        if (Minimum > Maximum)
            throw new ArgumentException("Minimum cannot be greater than Maximum");

        var value = Value;
        if (value < Minimum)
        {
            Value = Minimum;
        }
        else if (value > Maximum)
        {
            Value = Maximum;
        }
    }

    private void ValueTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = _numericRegex.IsMatch(e.Text);
    }

    private void ValueTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Up)
        {
            IncrementValue();
            e.Handled = true;
        }
        else if (e.Key == Key.Down)
        {
            DecrementValue();
            e.Handled = true;
        }
    }

    private void ValueTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text) || !int.TryParse(textBox.Text, out var value))
            {
                Value = Minimum;
            }
            else
            {
                Value = value;
            }
        }
    }

    private void UpButton_Click(object sender, RoutedEventArgs e)
    {
        IncrementValue();
    }

    private void DownButton_Click(object sender, RoutedEventArgs e)
    {
        DecrementValue();
    }

    private void IncrementValue()
    {
        var newValue = Value + Increment;
        if (newValue <= Maximum)
        {
            Value = newValue;
        }
    }

    private void DecrementValue()
    {
        var newValue = Value - Increment;
        if (newValue >= Minimum)
        {
            Value = newValue;
        }
    }

    private void NumericUpDown_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (e.Delta > 0)
        {
            IncrementValue();
        }
        else if (e.Delta < 0)
        {
            DecrementValue();
        }

        e.Handled = true;
    }
}