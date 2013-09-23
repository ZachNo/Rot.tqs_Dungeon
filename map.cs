$RotMap::doors::putIntoMap = 0;

if(!isObject(ROTMap))
	new scriptObject(ROTMap);
function ROTMap::init(%this,%width,%height)
{
	%this.width = %width;
	%this.height = %height;
//	%this.rooms = Array();
//	%this.corridors = Array();
}
function ROTMap::fillMap(%this,%value)
{
	%map = new scriptObject();
	for(%i=0;%i<%this.width;%i++)
	{
		for(%j=0;%j<%this.height;%j++)
			%map.m[%i,%j] = %value;
	}
	return %map;
}
function ROTMap::getRooms(%this)
{
	return %this.rooms;
}
function ROTMap::getCorridors(%this)
{
	return %this.corridors;
}
exec("./digger.cs");
exec("./features.cs");
function dumpMapToConsole()
{
	%line = "";
	for(%i=0;%i<RotMap.height;%i++)
	{
		for(%j=0;%j<RotMap.width;%j++)
			%line = %line @ RotMap.map.m[%j,%i];
		echo(%line);
		%line="";
	}
	//dumpRoomCoords();
}
function dumpRoomCoords()
{
	for(%i=1;%i<=RotMap.rooms.count;%i++)
	{
		%r = RotMap.rooms.r[%i];
		echo("Room" @ %i @ ": [" @ %r.getLeft() @ "," @ %r.getTop() @ "] to [" @ %r.getRight() @ "," @ %r.getBottom() @ "]");
		%r.getDoors();
	}
}
function dumpCorrCoords()
{
	for(%i=1;%i<=RotMap.corridors.count;%i++)
	{
		%c = RotMap.corridors.c[%i];
		echo("Corridor" @ %i @ ": [" @ %c.startx @ "," @ %c.starty @ "] to [" @ %c.endx @ "," @ %c.endy @ "]");
	}
}