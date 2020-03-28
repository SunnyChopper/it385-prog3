using System;
	
public struct IntPoint2D
{
	public int xCoord;
	public int yCoord;

	public static IntPoint2D zero = new IntPoint2D(0,0);

	public IntPoint2D(int x, int y)
	{
		xCoord = x;
		yCoord = y;
	}

	public static IntPoint2D operator +(IntPoint2D lhs, IntPoint2D rhs)
	{
		return new IntPoint2D(lhs.xCoord+rhs.xCoord,lhs.yCoord+rhs.yCoord);
	}

	public static bool operator==(IntPoint2D lhs, IntPoint2D rhs)
	{
		return (lhs.xCoord == rhs.xCoord && lhs.yCoord == rhs.yCoord);
	}

	public static bool operator!=(IntPoint2D lhs, IntPoint2D rhs)
	{
		return !(lhs == rhs);
	}

	public override bool Equals(object obj)
	{
		if (!(obj is IntPoint2D))
			return false;
		else 
		{
			IntPoint2D rhs = (IntPoint2D) obj;
			if (this.xCoord == rhs.xCoord) {
				if (this.yCoord == rhs.yCoord) {
					return true;
				} else {
					return false;
				}
			} else {
				return false;
			}
		}
	}

	public override int GetHashCode()
	{
		return this.xCoord+this.yCoord;
	}

	public override string ToString ()
	{
		return "(" + this.xCoord.ToString() + "," + this.yCoord.ToString() + ")";
	}
}

