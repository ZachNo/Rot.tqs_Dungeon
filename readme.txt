This isn't competely done yet, but Port wanted the code?
Basically the main issue is that is liked to make too many doors, so I restricted it and now it doesn't always connect all the rooms togehter making orphaned rooms.
So yeah.

Usage:
Execute map.cs
Use RotMap.call(x amount, y amount);
then call RotMap.create();
And it outputs the map to console.

Fun.

I found out where the doors issue is coming from.
When the rooms are initially generated it creates a door position, but it can make it in really weird places sometimes...