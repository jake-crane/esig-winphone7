using System;
using System.Windows;

/// <summary>
/// A TimedPoint class that is used to represent a location in coordinate space and it's time of creation.
/// The coordinates are stored using two double type variables.
/// The time is the number of milliseconds in UTC since January 1 1970 stored using a long variable.
/// </summary>
public class TimedPoint {

    public double X { get; set; }

    public double Y { get; set; }

    public long Time { get; set; }

    public TimedPoint(double x, double y) {
        X = x;
        Y = y;
        Time = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
    }

    public TimedPoint(Point p) {
        X = p.X;
        Y = p.Y;
        Time = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
    }

    public override string ToString() {
        return "{\"x\":" + X + ",\"y\":" + Y + ",\"time\":" + Time + "}";
    }

}
