using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public static class DifferentialNodesExtensions 
{
    public static void Traverse(this DifferentialLineScript.DifferentialNode[] nodes, System.Action<DifferentialLineScript.DifferentialNode> action)
    {
        var current = nodes[0];
        for (int i = 0; i < nodes.Length; i++)
        {
            action(current);
            current = nodes[current.next];
        }
    }
    public static void UnorderedTraverse(this DifferentialLineScript.DifferentialNode[] nodes, System.Action<DifferentialLineScript.DifferentialNode> action)
    {
        for (int i = 0; i < nodes.Length; i++)
        {
            action(nodes[i]);
        }
    }

    public static void UnorderedIndexTraverse(this DifferentialLineScript.DifferentialNode[] nodes, System.Action<DifferentialLineScript.DifferentialNode, int> action) 
    {
        for (int i = 0; i < nodes.Length; i++)
        {
            action(nodes[i], i);
        }
    }

    public static DifferentialLineScript.DifferentialNode NextNth(this DifferentialLineScript.DifferentialNode[] nodes, DifferentialLineScript.DifferentialNode start, int n) 
    {
        var result = start;
        for(int i = 0; i < n; i++) 
        {
            result = nodes[result.next];
        }
        return result;
    }
}


[RequireComponent(typeof(DifferentialLineScript))]
public class DifferentialLineExporter : MonoBehaviour
{
    DifferentialLineScript script;


    bool recording => frames.Count > 0;
    List<DifferentialLineScript.DifferentialNode[]> frames = new List<DifferentialLineScript.DifferentialNode[]>();

    DifferentialLineScript.DifferentialNode[] DownloadNodes()
    {
        var nodes = script.DownloadNodes();
        if (nodes.Length == 0)
        {
            throw new System.Exception("DifferentialLineExporter: nodes should never be empty!");
        }
        return nodes;
    }

    [Button]
    public void ExportFrameToSVG() 
    {
        var nodes = DownloadNodes();
        var vertices = new List<Vector2>();

        nodes.Traverse(node =>
        {
            vertices.Add(node.position);
        });

        SVGBuilder.New(script.outputDimensions)
            .AddPolygon(vertices).Build()
            .Output("output/svgs/single_frame");
    }

    [Header("Snapshot recording:")]
    [SerializeField]
    private float snapshotTime = 1.0f;
    [SerializeField, ReadOnly]
    private float accTime = .0f;

    [Button, HideIf(nameof(recording))]
    public void StartSnapshotRecording() 
    {
        if (recording) return;
        accTime = .0f;
        frames.Add(DownloadNodes());
    }

    [Button, ShowIf(nameof(recording))]
    public void EndSnapshotBasic() 
    {
        if (!recording) return;

        var builder = SVGBuilder.New(script.outputDimensions);

        var vertices = new List<Vector2>();
        foreach(var nodes in frames) 
        {
            nodes.Traverse(node =>
            {
                vertices.Add(node.position);
            });

            builder.AddPolygon(vertices);
            vertices.Clear();
        }

        builder.Build().Output("output/svgs/snapshots_basic");

        accTime = .0f;
        frames.Clear();
    }

    [Button, ShowIf(nameof(recording))]
    public void EndSnapshotClosestPattern()
    {
        if (!recording) return;

        var builder = SVGBuilder.New(script.outputDimensions);

        for(int frameIndex = 0; frameIndex < frames.Count; frameIndex++) 
        {
            var nodes = frames[frameIndex];

            if(frameIndex == 0 || frameIndex == frames.Count - 1)
            {
                var vertices = new List<Vector2>();
                nodes.Traverse(node =>
                {
                    vertices.Add(node.position);
                });
                builder.AddPolygon(vertices);
            }

            if (frameIndex != frames.Count - 1)
            {
                nodes.UnorderedTraverse(node => 
                {
                    float dist = 100000000.0f;
                    var smallestDistNodeIndex = 0;
                    var smallestDistFrameIndex = 0;
                    for(int i = frameIndex+1; i < frames.Count; i++) 
                    {
                        var nextNodes = frames[frameIndex + 1];
                        for (int nextNodeIndex = 0; nextNodeIndex < nextNodes.Length; nextNodeIndex++)
                        {
                            float currentDist = Vector2.Distance(node.position, nextNodes[nextNodeIndex].position);
                            if (currentDist < dist)
                            {
                                dist = currentDist;
                                smallestDistNodeIndex = nextNodeIndex;
                                smallestDistFrameIndex = i;
                            }
                        }
                    }

                    builder.AddLine(node.position, frames[smallestDistFrameIndex][smallestDistNodeIndex].position);
                });
            }
        }

        builder.Build().Output("output/svgs/snapshots_closest");

        accTime = .0f;
        frames.Clear();
    }


    [Button, ShowIf(nameof(recording))]
    public void EndSnapshotSameIndex()
    {
        if (!recording) return;

        var builder = SVGBuilder.New(script.outputDimensions);

        for (int frameIndex = 0; frameIndex < frames.Count; frameIndex++)
        {
            var nodes = frames[frameIndex];

            if (frameIndex == 0 || frameIndex == frames.Count - 1)
            {
                var vertices = new List<Vector2>();
                nodes.Traverse(node =>
                {
                    vertices.Add(node.position);
                });
                builder.AddPolygon(vertices);
            }

            if (frameIndex != frames.Count - 1)
            {
                nodes.UnorderedIndexTraverse((node, index) =>
                {
                    if(index % 2 == 0)
                    {
                        builder.AddLine(node.position, frames[frameIndex + 1][index].position);
                    }
                });
            }
        }

        builder.Build().Output("output/svgs/snapshots_index");

        accTime = .0f;
        frames.Clear();
    }



    [Button, ShowIf(nameof(recording))]
    public void EndSnapshotSmoothNodeControl()
    {
        if (!recording) return;


        {
            var builder = SVGBuilder.New(script.outputDimensions);

            var vertices = new List<Vector2>();
            foreach (var nodes in frames)
            {
                nodes.Traverse(node =>
                {
                    vertices.Add(node.position);
                });

                builder.AddPolygon(vertices);
                vertices.Clear();
            }

            builder.Build().Output("output/svgs/snapshots_basic");
        }

        {
            var builder = SVGBuilder.New(script.outputDimensions);

            for (int frameIndex = 0; frameIndex < frames.Count; frameIndex++)
            {
                var nodes = frames[frameIndex];

                var current = nodes[0];
                var path = builder.StartPath(current.position);
                for (int i = 0; i < nodes.Length; i += 2)
                {
                    var last = nodes.NextNth(current, 2);
                    path.QuadraticBezier(last.position, nodes[current.next].position);
                    current = last;
                }
                builder = path.EndPath();
            }

            builder.Build().Output("output/svgs/snapshots_smooth_node");

        }
        accTime = .0f;
        frames.Clear();
    }

    [SerializeField]
    float arbitraryControlDistance = 1.0f;

    [Button, ShowIf(nameof(recording))]
    public void EndSnapshotSmoothArbitraryControl()
    {
        if (!recording) return;

        {
            var builder = SVGBuilder.New(script.outputDimensions);

            var vertices = new List<Vector2>();
            foreach (var nodes in frames)
            {
                nodes.Traverse(node =>
                {
                    vertices.Add(node.position);
                });

                builder.AddPolygon(vertices);
                vertices.Clear();
            }

            builder.Build().Output("output/svgs/snapshots_basic");
        }

        {
            var builder = SVGBuilder.New(script.outputDimensions);

            for (int frameIndex = 0; frameIndex < frames.Count; frameIndex++)
            {
                var nodes = frames[frameIndex];

                Vector2 computeControl(DifferentialLineScript.DifferentialNode a, DifferentialLineScript.DifferentialNode b)
                {
                    var delta = b.position - a.position;
                    var middle = Vector2.Lerp(a.position, b.position, .5f);
                    var direction = delta.normalized;

                    return middle + new Vector2(direction.y, -direction.x) * arbitraryControlDistance;
                };

                var current = nodes[0];
                var path = builder.StartPath(current.position).QuadraticBezier(nodes[current.next].position, computeControl(current, nodes[current.next]));
                current = nodes[current.next];

                for (int i = 1; i < nodes.Length-1; i ++)
                {
                    path.ChainBezier(nodes[current.next].position);
                    current = nodes[current.next];
                }
                builder = path.Close().EndPath();
            }

            builder.Build().Output("output/svgs/snapshots_smooth_arbitrary");

        }
        accTime = .0f;
        frames.Clear();
    }

    // Start is called before the first frame update
    void Start()
    {
        script = GetComponent<DifferentialLineScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if (recording) 
        {
            accTime += Time.deltaTime;
            if(accTime >= snapshotTime) 
            {
                accTime -= snapshotTime;
                frames.Add(DownloadNodes());
            }
        }
    }
}
