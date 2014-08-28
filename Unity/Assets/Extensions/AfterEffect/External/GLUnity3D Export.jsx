{
        
var fileName;
var frameDuration;
 
var MAIN_STEAM = 1;
var SUB_STEAM = 2;

var outputString = '';
var subSteam = '';
var usedCompositionIds =  new Array();
var usedCompositions    =  new Array();
        

exportAnimation();    
    

     function exportAnimation() {
         for (var compIndex=app.project.selection.length-1; compIndex>=0; compIndex--)
         {
            var target = app.project.selection[compIndex];
             if (!(target instanceof CompItem)) {
                alert(target+" is not a composition, can't export");
                continue;
            }
            outputString = '';
            subSteam = '';
            usedCompositionIds = new Array();
            usedCompositions = new Array();
        
            fileName = target.name + '.xml';
            frameDuration = target.frameDuration;
            

            output('<?xml version="1.0" encoding="utf-8"?>', MAIN_STEAM);
            output('<after_affect_animation_doc>', MAIN_STEAM);
            exportLayers(target, MAIN_STEAM);
           
            
            
            
            output('<sub_items>', MAIN_STEAM);
            
            while(usedCompositions.length > 0) {
                comp = usedCompositions.pop();
                exportLayers(comp, MAIN_STEAM);
            }
            
            var totalFrames = Math.round(target.duration / frameDuration);
            
            output('</sub_items>', MAIN_STEAM);
            
            output('<meta frameDuration="' + frameDuration 
                           + '" totalFrames="' + totalFrames 
                           + '" duration="' + target.duration.toFixed(2)
                           
                           + '"/>', MAIN_STEAM);
            output('</after_affect_animation_doc>', MAIN_STEAM);    
              
            writeFile();
        }
     }
    

 	function exportLayers(composition, steam) {
        
        
	    output('<composition w="' + composition.width + 
                                        '" h="' + composition.height + 
                                        '" id="' + composition.id + 
                                        '" >', steam);
        for(var l_num = 1; l_num <= composition.numLayers; l_num++) {
            
              var layer = composition.layer(l_num);
              if(!layer.enabled) {
                continue;
              }

              
              var parentName = "none";
                 
              if(layer.parent != null) {
                  parentName = layer.parent.index;
              }

          
              var blending = string_of_enum(BlendingMode, layer.blendingMode)


              

              
             
               if(layer.source instanceof FootageItem) {
                  output('<layer name="' + layer.name + 
                                         '" type="' + layer.source.typeName + 
                                         '" parent="' +  parentName + 
                                         '" index="' + layer.index + 
                                         '" source="' + layer.source.file.name + 
                                         '" w="' + layer.width + 
                                         '" h="' + layer.height + 
                                         '" inPoint="' + layer.inPoint + 
                                         '" outPoint="' + layer.outPoint + 
                                         '" blending="' + blending + 
                                         '">', steam);
               } else if (layer.source instanceof CompItem) {
            
                    output('<layer name="' + layer.name + 
                                         '" type="' + layer.source.typeName + 
                                         '" id="'     + layer.source.id + 
                                         '" parent="' +  parentName + 
                                         '" index="' + layer.index + 
                                         '" w="' + layer.width + 
                                         '" h="' + layer.height + 
                                         '" inPoint="' + layer.inPoint + 
                                         '" outPoint="' + layer.outPoint + 
                                         '" blending="' + blending + 
                                         '">', steam);
                      
                       
                      if(!contains(usedCompositionIds, layer.source.id)) {
                            usedCompositions.push(layer.source);
                            usedCompositionIds.push(layer.source.id);
                      }

               } else {
                    alert("Object Type not supported: ");
                    continue;
               }
           
           
          
            
            
           
              var properties =  new Array();
              properties[0] = layer.Transform["Position"];
              properties[1] = layer.Transform["Scale"];
              properties[2] = layer.Transform["Rotation"];
              properties[3] = layer.Transform["Opacity"];
              properties[4] = layer.Transform["Anchor Point"];
              exportProperties(composition, properties, steam);
              
               
               
           
                                
              output('</layer>', steam);
       }
   
      var numFrames = Math.round(composition.duration / frameDuration);
      output('<meta duration="' + composition.duration.toFixed(2)
                                + '" totalFrames="' + numFrames 
                                + '"/>', steam);
   
        output('</composition>', steam);
            
	}

    function exportProperties(composition,  properties, steam) {
         var numFrames = Math.round(composition.duration / frameDuration);
		var i = 0;
		var l = numFrames;
		for (; i<l; ++i) {
           //time print disabled
			var time = i * frameDuration;
			// build properties list
			var x = 0;
			var y = properties.length;
			var source = [];
			var sourceCount = 0;
			var currentContainer;
			for (; x<y; ++x) {
				if (properties[x].valueAtTime != undefined) {
					var parentName;
					if (properties[x].parentProperty != undefined) {
						parentName = properties[x].parentProperty.name;
					}
					else {
						parentName = "Undefined Property";
					}
					if (source[parentName] == undefined) {
						source[parentName] = {};
						source[parentName].name = properties[x].name;
						source[parentName].values = [];
					}
					source[parentName].values.push(properties[x]);
				}
			}

            var outputtedKeyframe = false;

			// build xml
      
            for (var pn in source) {
                var outputtedSource = false;
                var x = 0;
                var y = source[pn].values.length;
                for (; x<y; ++x) {
                    var prop = source[pn].values[x];
                    if (i != 0)
                    {
                        var prevTime = (i-1) * frameDuration;
                        var val = prop.valueAtTime(time, true);
                        var prevVal = prop.valueAtTime(prevTime, true);
                        var sameValue = true;

                        var type = typeof(val);
                        if (typeof(val) == "number")
                        {
                            sameValue = (val == prevVal);
                        }
                        else
                        {
                            var valLength = val.length;
                            for (var k=0; k<valLength; ++k) {
                              if (val[k] != prevVal[k])
                              {
                                sameValue = false;
                                break;
                              }
                            }
                        }

                        if (sameValue)
                        {
                            continue;
                        }
                    }
                
                    if (!outputtedKeyframe)
                    {
                        output('<keyframe frame="' + i + '">', steam);
                        outputtedKeyframe = true; 
                    }
                    if (!outputtedSource)
                    {
                        output('<source name="' + pn + '">', steam);
                        outputtedSource = true;
                    }
                    exportProperty(prop, time, steam)
                }
                if (outputtedSource)
                {
                output('</source>', steam);
                }
            }
            if (outputtedKeyframe)
            {
                output('</keyframe>', steam); 
            }
        }
	}
    
    function exportProperty(prop, time, steam) {
		var val = prop.valueAtTime(time, true);
		if (val.length > 1) {
			exportMultiValue(prop, val, steam);
		}
		else {
			exportSingleValue(prop, val, steam);
		}
	}

	function exportSingleValue(prop, val, steam) {
		output('<property name="' + prop.name + '" val="' + val + '"/>', steam);
	}
	
	function exportMultiValue(prop, val,  steam) {
		var str = '<property name="' + prop.name + '"';
		
		var i = 0;
		var l = val.length;
		for (; i<l; ++i) {
             var name;
            switch (i) {
                case 0:
                    name = "x";
                    break;
                case 1:
                    name = "y";
                    break;
                case 2:
                    name = "z";
                    break;
            }
            str += '  ' +name + '="' + val[i] + '"';
		}
		
		str += '/>';
		output(str, steam);
	}
	
	function outputObject(obj) {
		var out = "";
		for (var s in obj) {
			out += s + "\n";
		}
		alert(out);
	}
 
    function output(value, steam) {
        if(steam == MAIN_STEAM) {
            outputString += value + "\r";
        } else {
             subSteam += value + "\r";
        }
	}
	
	function writeFile() {
		output('</composition>');
		var file = new File(Folder.desktop.absoluteURI + "/" + fileName);
		file.open("w","TEXT","????");
		file.write(outputString);
		file.close();
		//file.execute();
	}

    function string_of_enum(en,  value)  {
        for (var k in en) {
            if (en[k] == value) {
                return k;
            }
        } 
        return null;
    }

    function contains(a, obj) {
        var i = a.length;
        while (i--) {
           if (a[i] === obj) {
               return true;
           }
        }
        return false;
    }
     
}