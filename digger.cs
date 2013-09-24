function RotMap::call(%this,%width,%height,%options)
{
	RotMap.init(%width,%height);
	//Options
	%this.options = new scriptobject();
	%this.options.roomWidth = 3 SPC 9;
	%this.options.roomHeight = 3 SPC 5;
	%this.options.corridorLength = 3 SPC 10;
	%this.options.dugPercentage = 0.2;
	%this.options.timeLimit = 10000;
	//Features
	%this.features = new scriptobject();
	%this.features.room = 4;
	%this.features.corridor = 4;
	//Other
	%this.featureAttempts = 20;
	%this.walls = "";
	//Callbacks
}
function RotMap::create(%this)
{
	%this.rooms = new scriptObject(){count=0;};
	%this.corridors = new scriptObject(){count=0;};
	%this.map = %this.fillMap(1);
	%this.walls = new scriptObject(){xcount=%this.width;ycount=%this.height;};
	%this.dug = 0;
	%area = (%this.width-2)*(%this.height-2);
	
	%this.firstRoom();
	
	%t1 = getRealTime();
	//echo("Entering create map while loop");
	while(%this.dug/%area < %this.options.dugPercentage || %priorityWalls)
	{
		%t2 = getRealTime();
		if(%t2 - %t1 > %this.options.timeLimit)
		{
			//echo("Time limit exceeded");
			break;
		}
		%wall = %this.findWall();
		//echo("Wall " @ %wall);
		if(%wall == -1 || %wall $= "")
		{
			//echo("Ended wall nonexistant");
			break;
		}
		%parts = strReplace(%wall,"_"," ");
		%x = getWord(%parts,0);
		%y = getWord(%parts,1);
		%dir = %this.getDiggingDirection(%x,%y);
		if(%dir $= "")
			continue;
		%featureAttempts = 0;
		while(%featureAttempts < %this.featureAttempts)
		{
			%featureAttempts++;
			if(%this.tryFeature(%x,%y,getWord(%dir,0),getWord(%dir,1)))
			{
				%this.removeSurroundingWalls(%x,%y);
				%this.removeSurroundingWalls(%x-getWord(%dir,0),%y-getWord(%dir,1));
				//echo("Feature successful");
				break;
			}
		}
		%priorityWalls = 0;
		for(%i=0;%i<%this.walls.xcount;%i++)
		{
			for(%j=0;%j<%this.walls.ycount;%j++)
			{
				if(%this.walls.w[%i,%j] == 2)
				{
					//echo("Priority wall @" SPC %i SPC %j SPC %this.walls.w[%i,%j]);
					%priorityWalls++;
				}
			}
		}
	}
	//echo("Create ended:" SPC %this.dug/%area @"<"@ %this.options.dugPercentage);
	
	%this.addDoors();
	dumpMapToConsole();
	%r = %this.exportToKoG();
	
	%this.walls.delete();
	%this.map.delete();
	%this.rooms.delete();
	%this.corridors.delete();
	
	return %r;
}

function RotMap::digCallback(%this,%x,%y,%value)
{
	//echo("Digcallback:" SPC %x SPC %y SPC %value);
	if(%value == 0 || %value == 2)
	{
		%this.map.m[%x,%y] = %value;
		%this.dug++;
	}
	else
		%this.walls.w[%x,%y] = 1;
}

function RotMap::isWallCallback(%this,%x,%y)
{
	if(%x < 0 || %y < 0 || %x >= %this.width || %y >= %this.height)
		return 0;
	return (%this.map.m[%x,%y] == 1);
}

function RotMap::canBeDugCallback(%this,%x,%y)
{
	if(%x < 1 || %y < 1 || %x+1 >= %this.width || %y+1 >= %this.height)
		return 0;
	return (%this.map.m[%x,%y] == 1);
}

function RotMap::priorityWallCallback(%this,%x,%y)
{
	%this.walls.w[%x,%y] = 2;
}

function RotMap::firstRoom(%this)
{
	%cx = mfloor(%this.width/2);
	%cy = mfloor(%this.height/2);
	%room = RotMapFR.createRandomCenter(%cx,%cy,%this.options);
	%this.rooms.r[%this.rooms.count++] = %room;
	%room.create();
}

function RotMap::findWall(%this)
{
	%prio1 = new scriptObject(){count=0;};
	%prio2 = new scriptObject(){count=0;};
	for(%i=0;%i < %this.walls.xcount;%i++)
	{
		for(%j=0;%j<%this.walls.ycount;%j++)
		{
			%prio = %this.walls.w[%i @ "_" @ %j];
			//echo(%i SPC %j SPC %prio);
			if(%prio == 1 || %prio == 2)
			{
				//echo("Prio @" SPC %i SPC %j SPC ":" SPC %prio);
				if(%prio == 2)
					%prio2.p[%prio2.count++] = %i @ "_" @ %j;
				else
					%prio1.p[%prio1.count++] = %i @ "_" @ %j;
				//echo(%prio1.count SPC %prio2.count);
			}
		}
	}
	
	%arr = (%prio2.count ? %prio2 : %prio1);
	
	if(!%arr.count)
	{
		//echo("Count = 0!" SPC %arr.count);
		return -1;
	}
	%id = %arr.p[getRandom(1,%arr.count)];
	//echo("ID Check: " @ %id @ " " @ %this.walls.w[%id]);
	%this.walls.w[%id] = "";
	//echo("Wall blanked: " @ %this.walls.w[%id]);
	%prio1.delete();
	%prio2.delete();
	return %id; //Returns in format x_y
}

function RotMap::tryFeature(%this,%x,%y,%dx,%dy)
{
	%f = (getRandom(0,1) ? "R" : "C");
	%a = "RotMapF" @ %f;
	//echo("Trying feature create" SPC %a);
	%feature = %a.createRandomAt(%x,%y,%dx,%dy,%this.options);
	
	if(!%feature.isValid())
	{
		%feature.delete();
		//echo("Feature deleted, invalid");
		return 0;
	}
	//echo("Calling .create()" SPC %a);
	%feature.create();
	
	if(%f $= "R")
		%this.rooms.r[%this.rooms.count++] = %feature;
	if(%f $= "C")
	{
		%feature.createPriorityWalls();
		%this.corridors.c[%this.corridors.count++] = %feature;
	}
	
	return 1;
}

function RotMap::getDirs(%this,%i)
{
	if(!$RotMap::Dirs::init)
		initRotMapDirs();
	if(%i == 4)
		return $RotMap::Dirs::a4;
	if(%i == 6)
		return $RotMap::Dirs::a6;
	if(%i == 8)
		return $RotMap::Dirs::a8;
	return -1;
}

function initRotMapDirs()
{
	$RotMap::Dirs::a4 = new scriptObject()
	{
		d0 = 0 SPC -1;
		d1 = 1 SPC 0;
		d2 = 0 SPC 1;
		d3 = -1 SPC 0;
	};
	$RotMap::Dirs::a6 = new scriptObject()
	{
		d0 = -1 SPC -1;
		d1 = 1 SPC -1;
		d2 = 2 SPC 0;
		d3 = 1 SPC 1;
		d4 = -1 SPC 1;
		d5 = -2 SPC 0;
	};
	$RotMap::Dirs::a8 = new scriptObject()
	{
		d0 = 0 SPC -1;
		d1 = 1 SPC -1;
		d2 = 1 SPC 0;
		d3 = 1 SPC 1;
		d4 = 0 SPC 1;
		d5 = -1 SPC 1;
		d6 = -1 SPC 0;
		d7 = -1 SPC -1;
	};
	$RotMap::Dirs::init = 1;
}
function RotMap::removeSurroundingWalls(%this,%cx,%cy)
{
	%deltas = %this.getDirs(4);
	for(%i=0;%i<4;%i++)
	{
		%delta = %deltas.d[%i];
		%x = %cx + getWord(%delta,0);
		%y = %cy + getWord(%delta,1);
		%this.walls.w[%x,%y] = "";
		%x = %cx + 2*getWord(%delta,0);
		%y = %cy + 2*getWord(%delta,1);
		%this.walls.w[%x,%y] = "";
	}
}

function RotMap::getDiggingDirection(%this,%cx,%cy)
{
	%result = "";
	%deltas = %this.getDirs(4);
	for(%i=0;%i<4;%i++)
	{
		%delta = %deltas.d[%i];
		%x = %cx + getWord(%delta,0);
		%y = %cy + getWord(%delta,1);
		//echo("Digging Directions: (" @ %cx SPC %cy @ ") " @ %delta SPC ":" SPC %x SPC %y SPC ":" SPC %deltas);
		
		if(%x < 0 || %y < 0 || %x >= %this.width || %y >= %this.width)
			return "";
		
		if(%this.map.m[%x,%y] != 1)
		{
			if(%result)
				return "";
			%result = %delta;
		}
	}
	if(%result $="")
		return "";
	return -getWord(%result,0) SPC -getWord(%result,1);
}

function RotMap::addDoors(%this)
{
	for(%i=1;%i<=%this.rooms.count;%i++)
	{
		%room = %this.rooms.r[%i];
		%room.clearDoors();
		%room.addDoors();
	}
}