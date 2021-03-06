if(!isObject(ROTMapFR))
	new scriptObject(ROTMapFR){class=RotMapFeatureRoom;};
if(!isObject(ROTMapFC))
	new scriptObject(ROTMapFC){class=RotMapFeatureCorridor;};

function ROTMapFeatureRoom::init(%this,%x1,%y1,%x2,%y2,%doorx,%doory)
{
	%r = new scriptObject()
	{
		class = RotMapFeatureRoom;
	};
	%r.x1 = %x1;
	%r.x2 = %x2;
	%r.y1 = %y1;
	%r.y2 = %y2;
	%r.doors = new simGroup();
	//echo("Room creation:" SPC %x1 SPC %y1 SPC %x2 SPC %y2 SPC %doorx SPC %doory);
	if(%doorx >= 0 && %doory >= 0)
	{
		//echo("Init room with door @ (" @ %doorx SPC %doorx @ ")");
		%r.addDoor(%doorx,%doory);
	}
	return %r;
}

function RotMapFeatureRoom::createRandomAt(%this,%x,%y,%dx,%dy,%options)
{
	%min = getWord(%options.roomWidth,0);
	%max = getWord(%options.roomWidth,1);
	%width = %min + mFloor(getRandom()*(%max-%min+1));
	
	%min = getWord(%options.roomHeight,0);
	%max = getWord(%options.roomHeight,1);
	%height = %min + mFloor(getRandom()*(%max-%min+1));
	
	if(%dx == 1)
	{
		%y2 = %y - mFloor(getRandom() * %height);
		return %this.init(%x+1,%y2,%x+%width,%y2+%height-1,%x,%y);
	}
	
	if(%dx == -1)
	{
		%y2 = %y - mFloor(getRandom() * %height);
		return %this.init(%x-%width,%y2,%x-1,%y2+%height-1,%x,%y);
	}
	
	if(%dy == 1)
	{
		%x2 = %x - mFloor(getRandom() * %width);
		return %this.init(%x2,%y+1,%x2+%width-1,%y+%height,%x,%y);
	}
	
	if(%dy == -1)
	{
		%x2 = %x - mFloor(getRandom() * %width);
		return %this.init(%x2,%y-%height,%x2+%width-1,%y-1,%x,%y);
	}
}

function RotMapFeatureRoom::createRandomCenter(%this,%cx,%cy,%options)
{
	%min = getWord(%options.roomWidth,0);
	%max = getWord(%options.roomWidth,1);
	%width = %min + mFloor(getRandom(0,%max-%min+1));
	
	%min = getWord(%options.roomHeight,0);
	%max = getWord(%options.roomHeight,1);
	%height = %min + mFloor(getRandom(0,%max-%min+1));
	
	%x1 = %cx - mFloor(getRandom()*%width);
	%y1 = %cy - mFloor(getRandom()*%height);
	%x2 = %x1 + %width - 1;
	%y2 = %y1 + %height - 1;
	
	return %this.init(%x1,%y1,%x2,%y2);
}

function RotMapFeatureRoom::createRandom(%this,%availWidth,%availHeight,%options)
{
	%min = getWord(%options.roomWidth,0);
	%max = getWord(%options.roomWidth,1);
	%width = %min + mFloor(getRandom(0,%max-%min+1));
	
	%min = getWord(%options.roomHeight,0);
	%max = getWord(%options.roomHeight,1);
	%height = %min + mFloor(getRandom(0,%max-%min+1));
	
	%left = %availWidth - %width - 1;
	%top  =  %availHeight - %height - 1;
	
	%x1 = 1 + mFloor(getRandom()*%left);
	%y1 = 1 + mFloor(getRandom()*%top);
	%x2 = %x1 + %width - 1;
	%y2 = %y1 + %height - 1;
	
	return %this.init(%x1,%y1,%x2,%y2);
}

function RotMapFeatureRoom::addDoor(%this,%x,%y)
{
	//echo("Adding door(2) @ (" @ %x SPC %y @ ")");
	//%this.doors.d[%x,%y] = 1;
	%d = new scriptObject();
	%d.x = %x;
	%d.y = %y;
	%this.doors.add(%d);
	return %this;
}

function RotMapFeatureRoom::getDoors(%this,%callback)
{
	for(%i=0;%i<%this.doors.getCount();%i++)
	{
		%o = %this.doors.getObject(%i);
		echo("Door(" @ %o.x @ ", " @ %o.y @ ")");
	}
	return %this;
}

function RotMapFeatureRoom::clearDoors(%this)
{
	%this.doors.delete();
	%this.doors = new simGroup();
	return %this;
}

function RotMapFeatureRoom::addDoors(%this)
{
	%left = %this.x1-1;
	%right = %this.x2+1;
	%top = %this.y1-1;
	%bottom = %this.y2+1;
	
	for(%x=%left;%x<=%right;%x++)
	{
		for(%y=%top;%y<=%bottom;%y++)
		{
			if(%x != %left && %x != %right && %y != %top && %y != %bottom)
				continue;
			if(RotMap.map.m[%x,%y] == 1)
				continue;
			if(%x < 0 || %y < 0 || %x >= %this.width || %y >= %this.height)
				continue;
			
			%this.addDoor(%x,%y);
			//RotMap.digCallback(%x,%y,2);
		}
	}
	
	return %this;
}

function RotMapFeatureRoom::isValid(%this)
{
	%left = %this.x1-1;
	%right = %this.x2+1;
	%top = %this.y1-1;
	%bottom = %this.y2+1;
	
	for(%x=%left;%x<%right;%x++)
	{
		for(%y=%top;%y<%bottom;%y++)
		{
			if(%x == %left || %x ==  %right || %y == %top || %y ==  %bottom)
			{
				if(!RotMap.isWallCallback(%x,%y))
					return 0;
			}
			else
			{
				if(!RotMap.canBeDugCallback(%x,%y))
					return 0;
			}
		}
	}
	
	return 1;
}

function RotMapFeatureRoom::create(%this)
{
	//echo("Room creation called");
	%left = %this.x1-1;
	%right = %this.x2+1;
	%top = %this.y1-1;
	%bottom = %this.y2+1;
	
	%value = 0;
	for(%x=%left;%x<%right;%x++)
	{
		for(%y=%top;%y<%bottom;%y++)
		{
			if(%x == %left || %x ==  %right || %y == %top || %y ==  %bottom)
			{
				%value = 1;
			}
			else
				%value = 0;
			RotMap.digCallBack(%x,%y,%value);
			%c = %this.doors.getCount();
			for(%i=0;%i<%c;%i++)
			{
				%d = %this.doors.getObject(%i);
				//echo("Creating door @" SPC %d.x SPC %d.y);
				RotMap.digCallback(%d.x,%d.y,2);
			}
		}
	}
}

function RotMapFeatureRoom::getCenter(%this)
{
	return mFloatLength((%this.x1 + %this.x2)/2,0) SPC mFloatLength((%this.y1 + %this.y2)/2,0);
}

function RotMapFeatureRoom::getLeft(%this)
{
	return %this.x1;
}

function RotMapFeatureRoom::getRight(%this)
{
	return %this.x2;
}

function RotMapFeatureRoom::getTop(%this)
{
	return %this.y1;
}

function RotMapFeatureRoom::getBottom(%this)
{
	return %this.y2;
}

function RotMapFeatureCorridor::init(%this,%startx,%starty,%endx,%endy)
{
	%r = new scriptObject()
	{
		class = RotMapFeatureCorridor;
	};
	%r.startX = %startx;
	%r.startY = %starty;
	%r.endX = %endx;
	%r.endY = %endy;
	%r.endsWithAWall = 1;
	return %r;
}

function RotMapFeatureCorridor::createRandomAt(%this,%x,%y,%dx,%dy,%options)
{
	%min = getWord(%options.corridorLength,0);
	%max = getWord(%options.corridorLength,1);
	%length =  %min + mFloor(getRandom()*(%max-%min+1));
	
	return %this.init(%x,%y,%x + %dx*%length, %y + %dy*%length);
}

function RotMapFeatureCorridor::isValid(%this)
{
	%sx = %this.startX;
	%sy = %this.startY;
	%dx = %this.endX-%sx;
	%dy = %this.endY-%sy;
	if(mAbs(%dx) > mAbs(%dy))
		%length = 1 + mAbs(%dx);
	else
		%length = 1 + mAbs(%dy);
	
	if(%dx)
		%dx = %dx/mAbs(%dx);
	if(%dy)
		%dy = %dy/mAbs(%dy);
	%nx = %dy;
	%ny = -%dx;
		
	%ok = 1;
	for(%i=0;%i<%length;%i++)
	{
		%x = %sx + %i*%dx;
		%y = %sy + %i*%dy;
		
		if(!RotMap.canBeDugCallback(%x,%y))
			%ok=0;
		if(!RotMap.isWallCallback(%x+%nx,%y+%ny))
			%ok=0;
		if(!RotMap.isWallCallback(%x-%nx,%y-%ny))
			%ok=0;
		
		if(!%ok)
		{
			%length = %i;
			%this.endX = %x-%dx;
			%this.endY = %y-%dy;
			break;
		}
	}
	
	if(%length == 0)
		return 0;
	
	if(%length == 1 && RotMap.isWallCallback(%this.endx+%dx,%this.endy+%dy))
		return 0;
		
	%firstCornerBad = !RotMap.isWallCallback(%this.endx+%dx+%nx,%this.endy+%dy+%ny);
	%secondCornerBad = !RotMap.isWallCallback(%this.endx+%dx-%nx,%this.endy+%dy-%ny);
	%this.endsWithAWall = RotMap.isWallCallback(%this.endx+%dx,%this.endy+%dy);
	if((%firstCornerBad || %secondCornerBad) && %this.endsWithAWall)
		return 0;
	
	return 1;
}

function RotMapFeatureCorridor::create(%this)
{
	%sx = %this.startX;
	%sy = %this.startY;
	%dx = %this.endX-%sx;
	%dy = %this.endY-%sy;
	if(mAbs(%dx) > mAbs(%dy))
		%length = 1 + mAbs(%dx);
	else
		%length = 1 + mAbs(%dy);
	
	if(%dx)
		%dx = %dx/mAbs(%dx);
	if(%dy)
		%dy = %dy/mAbs(%dy);
	%nx = %dy;
	%ny = -%dx;
	
	for(%i=0;%i<%length;%i++)
	{
		%x = %sx + %i*%dx;
		%y = %sy + %i*%dy;
		RotMap.digCallback(%x,%y,2);
	}
	
	return 1;
}

function RotMapFeatureCorridor::createPriorityWalls(%this)
{
	if(!%this.endsWithAWall)
		return;
	%sx = %this.startX;
	%sy = %this.startY;
	%dx = %this.endX-%sx;
	%dy = %this.endY-%sy;
	
	if(%dx)
		%dx = %dx/mAbs(%dx);
	if(%dy)
		%dy = %dy/mAbs(%dy);
	%nx = %dy;
	%ny = -%dx;
	
	RotMap.priorityWallCallback(%this.endX+%dx,%this.endY+%dy);
	RotMap.priorityWallCallback(%this.endX+%nx,%this.endY+%ny);
	RotMap.priorityWallCallback(%this.endX-%nx,%this.endY-%ny);
}
