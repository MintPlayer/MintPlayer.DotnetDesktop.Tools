using Bacon.Core.Layers;

var image = new Bacon.Core.BaconIcon
{
    Images =
    [
        new Bacon.Core.BaconImage<int>
        {
            Width = 32,
            Height = 32,
            Layers =
            [
                new PaintLayer<int>{ },
                new ShapeLayer<int>
                {
                    Shapes =
                    [
                        new Bacon.Core.Shapes.Point<int>
                        {
                            Coordinate = new() { X = 5, Y = 10 }
                        },
                        new Bacon.Core.Shapes.Rectangle<int>
                        {
                            Location = new() { X = 6, Y = 10 },
                            Size = new() { Width = 4, Height = 2 },
                        }
                    ]
                },
            ]
        },
        new Bacon.Core.BaconImage<int>
        {
            Width = 64,
            Height = 64,
            Layers =
            [
                new ShapeLayer<int>
                {
                    Shapes =
                    [
                        new Bacon.Core.Shapes.Point<int>
                        {
                            Coordinate = new() { X = 2, Y = 4 }
                        },
                        new Bacon.Core.Shapes.Rectangle<int>
                        {
                            Location = new() { X = 3, Y = 5 },
                            Size = new() { Width = 2, Height = 1 },
                        }
                    ]
                },
            ]
        },
    ]
};

await image.Store(@"C:\Users\Pieterjan De Clippel\Pictures");