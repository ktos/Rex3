var canvas = document.getElementById("canvas");
var ctx = canvas.getContext("2d");
w = ctx.canvas.width = window.innerWidth;
h = ctx.canvas.height = window.innerHeight;

window.onresize = function() {
  w = ctx.canvas.width = window.innerWidth;
  h = ctx.canvas.height = window.innerHeight;
};

nt = 0;
noiseSpeed = 0.005;
noiseScale = 200;
dotSize = 8;
gap = 2;
hueBase = 200
hueRange = 60;
shape = 0;


function draw(){
  nt+=noiseSpeed;
  for(var x=0;x<w;x+=dotSize+gap){
    for(var y=0;y<h;y+=dotSize+gap){
      var yn = noise.perlin3(y/noiseScale, x/noiseScale, nt)*20;
      var cn = lerp(hueRange, yn*hueRange, 0.2);
      
      ctx.beginPath();
      ctx.fillStyle = "hsla("+(hueBase+cn)+",50%,50%,"+yn+")";
      if(shape==0){
        ctx.fillRect(x,y,dotSize,dotSize);
      }else if(shape==1){
        ctx.arc(x,y,dotSize/2,0,Math.PI*2);
        ctx.fill();
      } else if(shape==2){
        ctx.moveTo(x+(dotSize/2),y+dotSize);
        ctx.lineTo(x, y);
        ctx.lineTo(x+dotSize,y);
        ctx.fill();
      } else if(shape==3){
        if(y%((gap+dotSize)*2)==(gap+dotSize)){
          ctx.moveTo(x+(dotSize/2),y);
          ctx.lineTo(x+dotSize, y+dotSize);
          ctx.lineTo(x,y+dotSize);
        }else{
          ctx.moveTo(x+(dotSize/2),y+dotSize);
          ctx.lineTo(x, y);
          ctx.lineTo(x+dotSize,y);
        }
        ctx.fill();
      }
      ctx.closePath();
    }
  }
}

function clear(){
  ctx.fillStyle="rgba(0,0,0,1)";
  ctx.fillRect(0,0,w,h);
};

function lerp(x1, x2, n) {
  return (1 - n) * x1 + n * x2;
}

function render(){
  clear();
  draw();
  requestAnimationFrame(render);
}
render();