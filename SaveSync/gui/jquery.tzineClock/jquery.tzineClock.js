/*!
 * jquery.tzineClock.js - Tutorialzine Colorful Clock Plugin
 *
 * Copyright (c) 2009 Martin Angelov
 * http://tutorialzine.com/
 *
 * Licensed under MIT
 * http://www.opensource.org/licenses/mit-license.php
 *
 * Launch  : December 2009
 * Version : 1.0
 * Released: Monday 28th December, 2009 - 00:00
 */

	
// A global array used by the functions of the plug-in:
var gVars = {};

// Extending the jQuery core:
$.fn.tzineClock = function(opts) {

	// "this" contains the elements that were selected when calling the plugin: $('elements').tzineClock();
	// If the selector returned more than one element, use the first one:

	var container = this.eq(0);

	if (!container) {
	    try {
	        console.log("Invalid selector!");
	    } catch(e) { }

	    return false;
	}

	if (!opts) opts = { };

	var defaults = {
	    /* Additional options will be added in future versions of the plugin. */
	};

	/* Merging the provided options with the default ones (will be used in future versions of the plugin): */
	$.each(defaults, function(k, v) {
	    opts[k] = opts[k] || defaults[k];
	});

	// Calling the setUp function and passing the container,
	// will be available to the setUp function as "this":
	setUp.call(container);

	return this;
};
	
function setUp()
{	
	var tmp;
		
	// Creating a new element and setting the color as a class name:
	tmp = $('<div>').attr('class','blue clock').html(
		'<div class="display"></div>'+
			
		'<div class="front left"></div>'+
			
		'<div class="rotate left">'+
			'<div class="bg left"></div>'+
		'</div>'+
			
		'<div class="rotate right">'+
			'<div class="bg right"></div>'+
		'</div>'
	);
		
	// Appending to the container:
	$(this).append(tmp);
		
	// Assigning some of the elements as variables for speed:
	tmp.rotateLeft = tmp.find('.rotate.left');
	tmp.rotateRight = tmp.find('.rotate.right');
	tmp.display = tmp.find('.display');
		
	// Adding the dial as a global variable. Will be available as gVars.colorName
	gVars['blue'] = tmp;
		
	animation(gVars.blue,0, 100);
}
	
function animation(clock, current, total)
{
	if(clock == 1) {
		//console.log("Blue clock selected");
		clock = gVars.blue;
	}
	
	// Calculating the current angle:
	var angle = (360/total)*(current);
	
	var element;

	if(current==0)
	{
		console.log("Resetting the clock");
		// Hiding the right half of the background:
		clock.rotateRight.hide();
			
		// Resetting the rotation of the left part:
		rotateElement(clock.rotateLeft,0);
	}
		
	if(angle<=180)
	{
		console.log("Angle <= 180");
		// The left part is rotated, and the right is currently hidden:
		element = clock.rotateLeft;
	}
	else
	{
		console.log("Angle > 180");
		// The first part of the rotation has completed, so we start rotating the right part:
		clock.rotateRight.show();
		clock.rotateLeft.show();
			
		rotateElement(clock.rotateLeft,180);
			
		element = clock.rotateRight;
		angle = angle-180;
	}

	rotateElement(element,angle);

	// Setting the text inside of the display element, inserting a leading zero if needed:
	if (current != 0) {
		var filesToGo = total - current;
		clock.display.html(filesToGo);
	}
	else {
		clock.display.html(current);
	}
}
	
function rotateElement(element,angle)
{
	// Rotating the element, depending on the browser:
	var rotate = 'rotate('+angle+'deg)';
		
	if(element.css('MozTransform')!=undefined)
	{
		/*if(angle != 360 || angle != 0)
			element.css('-webkit-transition', 'all 0.5s ease-in-out');*/
			
		element.css('MozTransform', rotate);
	}
			
	else if(element.css('WebkitTransform')!=undefined)
	{
		/*if(angle!=0 || angle!=180)
			element.css('-webkit-transition', 'all 0s ease-in-out');
		else
			element.css('-webkit-transition', 'all 0s ease-in-out');*/
			
		element.css('WebkitTransform', rotate);
	}
	
	// A version for internet explorer using filters, works but is a bit buggy (no surprise here):
	else if(element.css("filter")!=undefined)
	{
		var cos = Math.cos(Math.PI * 2 / 360 * angle);
		var sin = Math.sin(Math.PI * 2 / 360 * angle);
			
		element.css("filter","progid:DXImageTransform.Microsoft.Matrix(M11="+cos+",M12=-"+sin+",M21="+sin+",M22="+cos+",SizingMethod='auto expand',FilterType='nearest neighbor')");
	
		element.css("left",-Math.floor((element.width()-200)/2));
		element.css("top",-Math.floor((element.height()-200)/2));
	}
	
}