#r "PresentationFramework.dll"
#r "PresentationCore.dll"
#r "WindowsBase.dll"
#r "System.Xaml.dll"

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Media
open System.Windows.Shapes

let canvas = new Canvas(Background = Brushes.Black)
let w = new Window(Topmost = true, Content = canvas)

w.Show();

[<Measure>]
type deg
[<Measure>]
type rad

type Turtle = {Position:Point; Angle:float<rad>; Color: Brush; IsUp: bool}

let turtle = {
    Position = new Point(canvas.ActualWidth/2.0, canvas.ActualHeight/2.0);
    Angle = 0.0<rad>;
    Color = Brushes.LightSteelBlue;
    IsUp = false }

let degToRad (deg:float<deg>) = LanguagePrimitives.FloatWithMeasure<rad> ((float deg)/180.0*Math.PI)
let radToDeg (rad:float<rad>) = LanguagePrimitives.FloatWithMeasure<deg> ((float rad)*180.0/Math.PI)

let arrowTranslation = new TranslateTransform()
let arrowRotation = new RotateTransform()


let arrow = 
    let points =
        [-10, 0; 10, 0; 0, 20] 
        |> Seq.map (fun (x, y) -> new Point (float x, float y))

    let pointCollection = new PointCollection(points);

    let arrowTransform = new TransformGroup()
    arrowTransform.Children.Add arrowRotation
    arrowTransform.Children.Add arrowTranslation

    new Polygon(
        Stroke = Brushes.White,
        Points = pointCollection,
        RenderTransform = arrowTransform 
        )

let updateArrowPosition turtle =
    arrowTranslation.X <- turtle.Position.X
    arrowTranslation.Y <- turtle.Position.Y

    arrowRotation.Angle <- 180.0 + (radToDeg >> float) turtle.Angle
    turtle

updateArrowPosition turtle
canvas.Children.Add arrow


let drawLine (destination:Point) turtle =
    let line = 
        new Line(
            Stroke = turtle.Color,
            X1 = turtle.Position.X,
            Y1 = turtle.Position.Y,
            X2 = destination.X,
            Y2 = destination.Y,
            StrokeThickness = 2.0)

    canvas.Children.Add line |> ignore

    { turtle with Position = destination }

let forward length turtle =
    let up = -length * cos (float turtle.Angle)
    let left = length * sin (float turtle.Angle)
    let destination = new Point(turtle.Position.X + left, turtle.Position.Y + up)
    if turtle.IsUp then updateArrowPosition { turtle with Position = destination }
    else drawLine destination turtle |> updateArrowPosition

let fd = forward

let rotate (angle:float<deg>) turtle =
    { turtle with Angle = (turtle.Angle + degToRad angle) % (2.0<rad>*Math.PI) }
    |> updateArrowPosition

let rt = rotate
let lt (angle:float<deg>) turtle = rotate (-angle) turtle

let repeat (n:int) (f:Turtle -> Turtle) turtle =
    let rec loop n t = 
        if n < 1 then t
        else loop (n-1) (f t)
    loop n turtle

let square length turtle = 
    repeat 4 (forward length >> rotate 90.0<deg>) turtle

let clear () = canvas.Children.Clear()