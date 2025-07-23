using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data;
using System.Runtime;
using Godot;
using System.IO;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Diagnostics.CodeAnalysis;
using NetworkMessages;

public struct Point
{
    public Point(double x, double y)
    {
        this.x = x;
        this.y = y;
    }
    public readonly double x;
    public readonly double y;
}

public struct Hex
{
    public Hex(int q, int r, int s)
    {
        this.q = q;
        this.r = r;
        this.s = s;
        if (q + r + s != 0) throw new ArgumentException("q + r + s must be 0");
    }
    public override string ToString()
    {
        return "("+q+", "+r+")";
    }

    public readonly int q;
    public readonly int r;
    public readonly int s;



    public Hex Add(Hex b)
    {
        return new Hex(q + b.q, r + b.r, s + b.s);
    }


    public Hex Subtract(Hex b)
    {
        return new Hex(q - b.q, r - b.r, s - b.s);
    }


    public Hex Scale(int k)
    {
        return new Hex(q * k, r * k, s * k);
    }


    public Hex RotateLeft()
    {
        return new Hex(-s, -q, -r);
    }


    public Hex RotateRight()
    {
        return new Hex(-r, -s, -q);
    }

    static public List<Hex> directions = new List<Hex>{new Hex(1, 0, -1), new Hex(1, -1, 0), new Hex(0, -1, 1), new Hex(-1, 0, 1), new Hex(-1, 1, 0), new Hex(0, 1, -1)};

    static public Hex Direction(int direction)
    {
        return Hex.directions[direction];
    }


    public Hex Neighbor(int direction)
    {
        return Add(Hex.Direction(direction));
    }

    public Hex WrappingNeighbor(int direction, int left, int right)
    {
        Hex hex = Add(Direction(direction));
        return hex.WrapHex(left - (hex.r >> 1), right - (hex.r >> 1));
    }

    public Hex WrapHex()
    {
        int left = Global.gameManager.game.mainGameBoard.left - (r >> 1);
        int right = Global.gameManager.game.mainGameBoard.right - (r >> 1);
        return WrapHex(left, right);
    }

    public Hex WrapHex(int left, int right)
    {
        int range = right - left;
        int newQ = ((q - left) % range) + left;
        if(newQ < left)
        {
            newQ = right + ((newQ-left)% range);
        }
        return new Hex(newQ, r, -newQ - r);
    }

    public Hex[] Neighbors()
    {
        Hex[] neighbors =
        [
            Neighbor(0),
            Neighbor(1),
            Neighbor(2),
            Neighbor(3),
            Neighbor(4),
            Neighbor(5),
        ];
        return neighbors;
    }

    public Hex[] WrappingNeighbors(int left, int right, int bottom)
    {
        List<Hex> neightborList = new List<Hex> ();
        for (int i = 0; i < 6; i++)
        {
            Hex temp = WrappingNeighbor(i, left, right);
            if (temp.r >= 0 && temp.r < bottom-1)
            {
                neightborList.Add(temp);
            }
        }
        Hex[] neighbors = neightborList.ToArray();
        return neighbors;
    }

    static public List<Hex> diagonals = new List<Hex>{new Hex(2, -1, -1), new Hex(1, -2, 1), new Hex(-1, -1, 2), new Hex(-2, 1, 1), new Hex(-1, 2, -1), new Hex(1, 1, -2)};

    public Hex DiagonalNeighbor(int direction)
    {
        return Add(Hex.diagonals[direction]);
    }

    public List<Hex> Range(int range)
    {
        List<Hex> results = new();
        for (int q = -range; q <= range; q++)
        {
            for (int r = Math.Max(-range, -q - range); r <= Math.Min(range, -q + range); r++)
            {
                int s = -q - r;
                results.Add(new Hex(q, r, s));
            }
        }
        return results;
    }

public List<Hex> WrappingRange(int range, int left, int right, int top, int bottom)
{
    List<Hex> results = new();
    int width = right - left;
    for (int q = -range; q <= range; q++)
    {
        for (int r = Math.Max(-range, -q - range); r <= Math.Min(range, -q + range); r++)
        {
                // Adjust the q coordinate using modular arithmetic for wrapping.
                int rangeQ = q + this.q;
                int rangeR = r + this.r; // Offset r by the origin's r coordinate.
                int rangeS = -rangeQ - rangeR; // Calculate s based on wrapped q and r.

                // Check bounds for r and add the hex to the results.
                if (rangeR >= top && rangeR < bottom-1)
                {                    
                    Hex hex = new Hex(rangeQ, rangeR, rangeS).WrapHex();
                    results.Add(hex);
                }
        }
    }
    return results;
}


    public int Length()
    {
        return (int)((Math.Abs(q) + Math.Abs(r) + Math.Abs(s)) / 2);
    }


    public int Distance(Hex b)
    {
        return Subtract(b).Length();
    }

    public int WrapDistance(Hex b)
    {
        int leftBoundary = Global.gameManager.game.mainGameBoard.left - (b.r >> 1);
        int rightBoundary = Global.gameManager.game.mainGameBoard.right - (b.r >> 1);
        return WrapDistance(b, leftBoundary, rightBoundary);
    }

    public int WrapDistance(Hex targetHex, int mapLeftBound, int mapRightBound)
    {
        // 1. Calculate the map's total width correctly.
        // If the map runs from column 0 to 10 (exclusive), the width is 10.
        int mapWidth = mapRightBound - mapLeftBound;

        // 2. Consider three possibilities for the target hex's Q coordinate:
        //    - Its original position.
        //    - Wrapped once to the right.
        //    - Wrapped once to the left.
        int qOriginal = targetHex.q;
        int qWrappedRight = targetHex.q + mapWidth;
        int qWrappedLeft = targetHex.q - mapWidth;

        // 3. Find which of the three Q coordinates is closest to our own Q.
        int deltaQOriginal = Math.Abs(this.q - qOriginal);
        int deltaQRight = Math.Abs(this.q - qWrappedRight);
        int deltaQLeft = Math.Abs(this.q - qWrappedLeft);

        int unwrappedQ = qOriginal;
        if (deltaQRight < deltaQOriginal)
        {
            unwrappedQ = qWrappedRight;
        }
        if (deltaQLeft < Math.Abs(this.q - unwrappedQ))
        {
            unwrappedQ = qWrappedLeft;
        }

        // 4. Create a "ghost" hex using the closest Q coordinate.
        Hex unwrappedHex = new Hex(unwrappedQ, targetHex.r, -unwrappedQ - targetHex.r);

        // 5. Return the standard, non-wrapping distance to this new ghost hex.
        return this.Distance(unwrappedHex);
    }
}

public struct FractionalHex
{
    public FractionalHex(double q, double r, double s)
    {
        this.q = q;
        this.r = r;
        this.s = s;
        if (Math.Round(q + r + s) != 0) throw new ArgumentException("q + r + s must be 0");
    }
    public readonly double q;
    public readonly double r;
    public readonly double s;

    public Hex HexRound()
    {
        int qi = (int)(Math.Round(q));
        int ri = (int)(Math.Round(r));
        int si = (int)(Math.Round(s));
        double q_diff = Math.Abs(qi - q);
        double r_diff = Math.Abs(ri - r);
        double s_diff = Math.Abs(si - s);
        if (q_diff > r_diff && q_diff > s_diff)
        {
            qi = -ri - si;
        }
        else
            if (r_diff > s_diff)
            {
                ri = -qi - si;
            }
            else
            {
                si = -qi - ri;
            }
        return new Hex(qi, ri, si);
    }



    public FractionalHex HexLerp(FractionalHex b, double t)
    {
        return new FractionalHex(q * (1.0 - t) + b.q * t, r * (1.0 - t) + b.r * t, s * (1.0 - t) + b.s * t);
    }


    static public List<Hex> HexLinedraw(Hex a, Hex b)
    {
        int N = a.Distance(b);
        FractionalHex a_nudge = new FractionalHex(a.q + 1e-06, a.r + 1e-06, a.s - 2e-06);
        FractionalHex b_nudge = new FractionalHex(b.q + 1e-06, b.r + 1e-06, b.s - 2e-06);
        List<Hex> results = new List<Hex>{};
        double step = 1.0 / Math.Max(N, 1);
        for (int i = 0; i <= N; i++)
        {
            results.Add(a_nudge.HexLerp(b_nudge, step * i).HexRound());
        }
        return results;
    }

}

public struct OffsetCoord
{
    public OffsetCoord(int col, int row)
    {
        this.col = col;
        this.row = row;
    }
    public readonly int col; 
    public readonly int row;
    static public int EVEN = 1;
    static public int ODD = -1;

    public override string ToString()
    {
        return "(" + row + ", " + col + ")";
    }

    static public OffsetCoord QoffsetFromCube(int offset, Hex h)
    {
        int col = h.q;
        int row = h.r + (int)((h.q + offset * (h.q & 1)) / 2);
        if (offset != OffsetCoord.EVEN && offset != OffsetCoord.ODD)
        {
            throw new ArgumentException("offset must be EVEN (+1) or ODD (-1)");
        }
        return new OffsetCoord(col, row);
    }


    static public Hex QoffsetToCube(int offset, OffsetCoord h)
    {
        int q = h.col;
        int r = h.row - (int)((h.col + offset * (h.col & 1)) / 2);
        int s = -q - r;
        if (offset != OffsetCoord.EVEN && offset != OffsetCoord.ODD)
        {
            throw new ArgumentException("offset must be EVEN (+1) or ODD (-1)");
        }
        return new Hex(q, r, s);
    }


    static public OffsetCoord RoffsetFromCube(int offset, Hex h)
    {
        int col = h.q + (int)((h.r + offset * (h.r & 1)) / 2);
        int row = h.r;
        if (offset != OffsetCoord.EVEN && offset != OffsetCoord.ODD)
        {
            throw new ArgumentException("offset must be EVEN (+1) or ODD (-1)");
        }
        return new OffsetCoord(col, row);
    }


    static public Hex RoffsetToCube(int offset, OffsetCoord h)
    {
        int q = h.col - (int)((h.row + offset * (h.row & 1)) / 2);
        int r = h.row;
        int s = -q - r;
        if (offset != OffsetCoord.EVEN && offset != OffsetCoord.ODD)
        {
            throw new ArgumentException("offset must be EVEN (+1) or ODD (-1)");
        }
        return new Hex(q, r, s);
    }

}

public struct DoubledCoord
{
    public DoubledCoord(int col, int row)
    {
        this.col = col;
        this.row = row;
    }
    public readonly int col;
    public readonly int row;

    static public DoubledCoord QdoubledFromCube(Hex h)
    {
        int col = h.q;
        int row = 2 * h.r + h.q;
        return new DoubledCoord(col, row);
    }


    public Hex QdoubledToCube()
    {
        int q = col;
        int r = (int)((row - col) / 2);
        int s = -q - r;
        return new Hex(q, r, s);
    }


    static public DoubledCoord RdoubledFromCube(Hex h)
    {
        int col = 2 * h.q + h.r;
        int row = h.r;
        return new DoubledCoord(col, row);
    }


    public Hex RdoubledToCube()
    {
        int q = (int)((col - row) / 2);
        int r = row;
        int s = -q - r;
        return new Hex(q, r, s);
    }

}

public struct Orientation
{
    public Orientation(double f0, double f1, double f2, double f3, double b0, double b1, double b2, double b3, double start_angle)
    {
        this.f0 = f0;
        this.f1 = f1;
        this.f2 = f2;
        this.f3 = f3;
        this.b0 = b0;
        this.b1 = b1;
        this.b2 = b2;
        this.b3 = b3;
        this.start_angle = start_angle;
    }
    public readonly double f0;
    public readonly double f1;
    public readonly double f2;
    public readonly double f3;
    public readonly double b0;
    public readonly double b1;
    public readonly double b2;
    public readonly double b3;
    public readonly double start_angle;
}

public struct Layout
{
    public Layout(Orientation orientation, Point size, Point origin)
    {
        this.orientation = orientation;
        this.size = size;
        this.origin = origin;
    }
    public readonly Orientation orientation;
    public readonly Point size;
    public readonly Point origin;
    static public Orientation pointy = new Orientation(Math.Sqrt(3.0), Math.Sqrt(3.0) / 2.0, 0.0, 3.0 / 2.0, Math.Sqrt(3.0) / 3.0, -1.0 / 3.0, 0.0, 2.0 / 3.0, 0.5);
    static public Orientation flat = new Orientation(3.0 / 2.0, 0.0, Math.Sqrt(3.0) / 2.0, Math.Sqrt(3.0), 2.0 / 3.0, 0.0, -1.0 / 3.0, Math.Sqrt(3.0) / 3.0, 0.0);

    public Point HexToPixel(Hex h)
    {
        Orientation M = orientation;
        double x = (M.f0 * h.q + M.f1 * h.r) * size.x;
        double y = (M.f2 * h.q + M.f3 * h.r) * size.y;
        return new Point(x + origin.x, y + origin.y);
    }

    public Point FractionalHexToPixel(FractionalHex h)
    {
        Orientation M = orientation;
        double x = (M.f0 * h.q + M.f1 * h.r) * size.x;
        double y = (M.f2 * h.q + M.f3 * h.r) * size.y;
        return new Point(x + origin.x, y + origin.y);
    }


    public FractionalHex PixelToHex(Point p)
    {
        Orientation M = orientation;
        Point pt = new Point((p.x - origin.x) / size.x, (p.y - origin.y) / size.y);
        double q = M.b0 * pt.x + M.b1 * pt.y;
        double r = M.b2 * pt.x + M.b3 * pt.y;
        return new FractionalHex(q, r, -q - r);
    }


    public Point HexCornerOffset(int corner)
    {
        Orientation M = orientation;
        double angle = 2.0 * Math.PI * (M.start_angle - corner) / 6.0;
        return new Point(size.x * Math.Cos(angle), size.y * Math.Sin(angle));
    }


    public List<Point> PolygonCorners(Hex h)
    {
        List<Point> corners = new List<Point>{};
        Point center = HexToPixel(h);
        for (int i = 0; i < 6; i++)
        {
            Point offset = HexCornerOffset(i);
            corners.Add(new Point(center.x + offset.x, center.y + offset.y));
        }
        return corners;
    }

}

public sealed class HexJsonConverter : JsonConverter<Hex>
{
    public override Hex Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        String value = reader.GetString();
        string[] parts = value.Split(',');

        return new Hex(int.Parse(parts[0].Trim()), int.Parse(parts[1].Trim()), int.Parse(parts[2].Trim()));
    }
    public override void Write(Utf8JsonWriter writer, Hex hex, JsonSerializerOptions options)
    {
        writer.WriteStringValue($"{hex.q},{hex.r},{hex.s}");
    }

    public override void WriteAsPropertyName(Utf8JsonWriter writer, Hex hex, JsonSerializerOptions options)
    {
        writer.WritePropertyName($"{hex.q},{hex.r},{hex.s}");
    }

    public override Hex ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        String value = reader.GetString();
        string[] parts = value.Split(',');

        return new Hex(int.Parse(parts[0].Trim()), int.Parse(parts[1].Trim()), int.Parse(parts[2].Trim()));
    }
}
