using System.Windows;
using System.Windows.Media.Animation;

namespace ImageViewer.Helpers;

public class ProgressAnimationHelper : DependencyObject
{
    public static readonly DependencyProperty AnimatedValueProperty =
        DependencyProperty.Register(nameof(AnimatedValue), typeof(double), typeof(ProgressAnimationHelper),
            new PropertyMetadata(0.0, OnAnimatedValueChanged));

    private WeakReference<Action<double>>? _updateCallback;
    private Storyboard? _currentStoryboard;

    public double AnimatedValue
    {
        get => (double)GetValue(AnimatedValueProperty);
        set => SetValue(AnimatedValueProperty, value);
    }

    public void SetUpdateCallback(Action<double> callback)
    {
        _updateCallback = callback != null ? new WeakReference<Action<double>>(callback) : null;
    }

    public void StartAnimation(double fromValue, double toValue, TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero)
            throw new ArgumentException("Duration must be positive", nameof(duration));

        StopAnimation();

        var animation = new DoubleAnimation
        {
            From = fromValue,
            To = toValue,
            Duration = duration
        };

        _currentStoryboard = new Storyboard();
        _currentStoryboard.Children.Add(animation);

        Storyboard.SetTarget(animation, this);
        Storyboard.SetTargetProperty(animation, new PropertyPath(AnimatedValueProperty));

        _currentStoryboard.Begin();
    }

    public void StopAnimation()
    {
        if (_currentStoryboard != null)
        {
            _currentStoryboard.Stop();
            _currentStoryboard = null;
        }
    }

    public void ResetValue()
    {
        StopAnimation();
        AnimatedValue = 0.0;
    }

    private static void OnAnimatedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ProgressAnimationHelper helper && helper._updateCallback != null)
        {
            if (helper._updateCallback.TryGetTarget(out var callback))
            {
                callback.Invoke((double)e.NewValue);
            }
        }
    }
}