using System;
using System.Windows;

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
