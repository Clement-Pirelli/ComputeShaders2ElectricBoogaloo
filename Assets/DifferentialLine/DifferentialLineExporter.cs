using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[RequireComponent(typeof(DifferentialLineScript))]
public class DifferentialLineExporter : MonoBehaviour
{
    DifferentialLineScript script;


    [SerializeField]
    private float snapshotTime = 1.0f;
    private float accTime = .0f;
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

    void Traverse(DifferentialLineScript.DifferentialNode[] nodes, System.Action<DifferentialLineScript.DifferentialNode> action) 
    {
        var current = nodes[0];
        for (int i = 0; i < nodes.Length; i++)
        {
            action(current);
            current = nodes[current.next];
        }
    }

    [Button]
    public void ExportFrameToSVG() 
    {
        var nodes = DownloadNodes();
        var vertices = new List<Vector2>();

        Traverse(nodes, node =>
        {
            vertices.Add(node.position);
        });

        SVGBuilder.New(script.outputDimensions)
            .AddPolygon(vertices).Build()
            .Output("output/svgs/single_frame");
    }
    
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
            Traverse(nodes, node =>
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
