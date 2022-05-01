using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Linq;
using System.IO;

public class SVGBuilder
{
    private class XMLAttribute
    {
        public XMLAttribute(string value, string name) { this.value = value; this.name = name; }
        public string value;
        public string name;
    }

    private static XMLAttribute Attribute<T>(string name, T value) 
    {
        return new XMLAttribute(value.ToString(), name);
    }

    private static string Tag(string name, IEnumerable<XMLAttribute> attributes)
    {
        string result = $"<{name} ";
        foreach (var attribute in attributes)
        {
            result += $"{attribute.name}=\"{attribute.value}\" ";
        }
        return result + "/>";
    }

    private StringBuilder contents;
    private SVGBuilder(string initialContents)
    {
        contents = new StringBuilder(initialContents);
    }
    public SVGBuilder AddLine(Vector2 start, Vector2 end) 
    {
        var attributes = new XMLAttribute[]
        {
            Attribute("x1", start.x),
            Attribute("y1", start.y),
            Attribute("x2", end.x),
            Attribute("y2", end.y),
            Attribute("stroke", "black")
        };
        contents.AppendLine(Tag("line", attributes));
        return this;
    }

    public SVGBuilder AddPolygon(List<Vector2> vertices) 
    {
        var points = vertices.Select(v => $"{v.x},{v.y} ").Aggregate((f, s) => f + s);

        var attributes = new XMLAttribute[]
        {
            Attribute("points", points),
            Attribute("stroke", "black"),
            Attribute("fill", "none")
        };
        contents.AppendLine(Tag("polygon", attributes));
        return this;
    }

    public SVGBuilder AddCircle(Vector2 origin, float size, bool fill = false)
    {
        var attributes = new XMLAttribute[]
        {
            Attribute("cx", origin.x),
            Attribute("cy", origin.y),
            Attribute("r", size),
            Attribute("stroke", "black"),
            Attribute("fill", fill ? "black" : "none")
        };
        contents.AppendLine(Tag("circle", attributes));
        return this;
    }

    public SVGBuilder Ellipse(Vector2 origin, Vector2 size, bool fill = false)
    {
        var attributes = new XMLAttribute[]
        {
            Attribute("cx", origin.x),
            Attribute("cy", origin.y),
            Attribute("rx", size.x),
            Attribute("ry", size.y),
            Attribute("stroke", "black"),
            Attribute("fill", fill ? "black" : "none")
        };
        contents.AppendLine(Tag("ellipse", attributes));
        return this;
    }

    public static SVGBuilder New(Vector2Int dimensions) 
    {
        return new SVGBuilder($"<svg version=\"1.1\"\nwidth=\"{dimensions.x}\"\nheight=\"{dimensions.y}\"\nxmlns=\"http://www.w3.org/2000/svg\">\n");
    }

    public PathBuilder StartPath(Vector2 startPosition) 
    {
        return new PathBuilder(this, startPosition);
    }

    public class PathBuilder
    {
        private SVGBuilder svg;
        private StringBuilder contents = new StringBuilder();
        bool previousWasBezier = false;
        public PathBuilder(SVGBuilder svg, Vector2 startingPoint) 
        {
            this.svg = svg;
            MoveTo(startingPoint);
        }

        public PathBuilder MoveTo(Vector2 to)
        {
            contents.Append($"M {to.x} {to.y} ");
            return this;
        }

        public PathBuilder QuadraticBezier(Vector2 to, Vector2 control)
        {
            contents.Append($"Q {control.x} {control.y}, {to.x} {to.y}");
            previousWasBezier = true;
            return this;
        }

        public PathBuilder ChainBezier(Vector2 to) 
        {
            //todo: this API is error prone, make SVGBuilder partial, split PathBuilder to its own file, make the bezier thing a builder of its own
            if(previousWasBezier == false) 
            {
                throw new System.Exception("ChainBezier called when the previous instruction was not a bezier!");
            }
            //no need to set previousWasBezier here, it's guaranteed to be true
            contents.Append($"T {to.x} {to.y}");
            return this;
        }

        public PathBuilder Close() 
        {
            contents.Append("Z ");
            previousWasBezier = false;
            return this;
        }

        public SVGBuilder EndPath() 
        {
            var attributes = new XMLAttribute[]
            {
                Attribute("d", contents),
                Attribute("stroke", "black"),
                Attribute("fill", "none"),
            };
            svg.contents.AppendLine(Tag("path", attributes));
            return svg;
        }
    }


    public SVGFile Build() 
    {
        return new SVGFile(contents.AppendLine("</svg>").ToString());
    }
}

public class SVGFile 
{
    string contents;
    public bool Output(string path) 
    {
        string modified_path = path;
        int attempt = 1;
        while (File.Exists(modified_path + ".svg"))
        {
            modified_path = $"{path}_{attempt}";
            attempt++;
        }

        try
        {
            using (StreamWriter writer = File.CreateText(modified_path + ".svg"))
            {
                writer.Write(contents);
            }
            return true;
        }
        catch(System.Exception) 
        {
            return false;
        }
    }

    public SVGFile(string contents) 
    {
        this.contents = contents;
    }
}
