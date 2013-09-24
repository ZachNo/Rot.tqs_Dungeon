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
function RotMap::exportToKoG(%this)
{
	%dungeon = Dungeon(%this.width,%this.height);
	for(%y=0;%y<%this.height;%y++)
	{
		for(%x=0;%x<%this.width;%x++)
		{
			%obj = "";
			%data = %this.map.m[%x,%y];
			switch$ (%data)
			{
				case 0: %obj = Room();
				case 2: %obj = Corridor();
			}
		}
	}
	if(isObject(%obj))
		%dungeon.setGridObject(%x,%y,%obj);
	return %dungeon;
}

function generateDiggerDungeon(%width,%height)
{
	RotMap.call(%width,%height);
	%dungeon = RotMap.create();
	return %dungeon;
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