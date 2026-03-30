using System.Numerics;
using Clipper2Lib;
using ShapeEngine.Geometry.PolygonDef;

namespace ShapeEngine.ShapeClipper;

public class ShapeClipper64
{
    private readonly Clipper64 clipEngine;
    private readonly Paths64PooledBuffer paths64SubjectBuffer = new();
    private readonly Paths64PooledBuffer paths64ClipBuffer = new();
    private readonly Path64 path64SubjectBuffer = new();
    private readonly Path64 path64ClipBuffer = new();
    private readonly Paths64 paths64SolutionBuffer = new();
    
    public ShapeClipperFillRule FillRule = ShapeClipperFillRule.NonZero;

    public bool PreserveCollinear
    {
        get => clipEngine.PreserveCollinear;
        set => clipEngine.PreserveCollinear = value;
    }

    public bool ReverseSolution
    {
        get => clipEngine.ReverseSolution;
        set => clipEngine.ReverseSolution = value;
    }

    public ShapeClipper64(bool preserveCollinear = true, bool reverseSolution = false)
    {
        clipEngine = new();
        clipEngine.PreserveCollinear = preserveCollinear;
        clipEngine.ReverseSolution = reverseSolution;
    }
    
    public void Execute(Paths64 subject, Paths64 clip, ShapeClipperClipType clipType, Paths64 solutionClosed)
    {
        clipEngine.Clear();
        clipEngine.AddSubject(subject);
        clipEngine.AddClip(clip);
        clipEngine.Execute(clipType.ToClipperClipType(), FillRule.ToClipperFillRule(), solutionClosed);
    }
    
    public void Execute(Paths64 subject, Path64 clip, ShapeClipperClipType clipType, Paths64 solutionClosed)
    {
        clipEngine.Clear();
        clipEngine.AddSubject(subject);
        clipEngine.AddClip(clip);
        clipEngine.Execute(clipType.ToClipperClipType(), FillRule.ToClipperFillRule(), solutionClosed);
    }
    
    public void Execute(Path64 subject, Paths64 clip, ShapeClipperClipType clipType, Paths64 solutionClosed)
    {
        clipEngine.Clear();
        clipEngine.AddSubject(subject);
        clipEngine.AddClip(clip);
        clipEngine.Execute(clipType.ToClipperClipType(), FillRule.ToClipperFillRule(), solutionClosed);
    }
    
    public void Execute(Path64 subject, Path64 clip, ShapeClipperClipType clipType, Paths64 solutionClosed)
    {
        clipEngine.Clear();
        clipEngine.AddSubject(subject);
        clipEngine.AddClip(clip);
        clipEngine.Execute(clipType.ToClipperClipType(), FillRule.ToClipperFillRule(), solutionClosed);
    }
    
    
    public void ExecuteMany(Paths64 subject, Paths64 clips, ShapeClipperClipType clipType, Paths64 solutionClosed)
    {
        var clipperClipType = clipType.ToClipperClipType();
        var clipperFillRule = FillRule.ToClipperFillRule();
        bool started = false;
        foreach (var c in clips)
        {
            clipEngine.Clear();
            clipEngine.AddSubject(started ? solutionClosed : subject);
            clipEngine.AddClip(c);
            clipEngine.Execute(clipperClipType, clipperFillRule, solutionClosed);

            if (!started) started = true;
        }
    }
    
    public void ExecuteMany(Path64 subject, Paths64 clips, ShapeClipperClipType clipType, Paths64 solutionClosed)
    {
        var clipperClipType = clipType.ToClipperClipType();
        var clipperFillRule = FillRule.ToClipperFillRule();
        bool started = false;
        foreach (var c in clips)
        {
            clipEngine.Clear();
            if(started)clipEngine.AddSubject(solutionClosed);
            else clipEngine.AddSubject(subject);
            clipEngine.AddClip(c);
            clipEngine.Execute(clipperClipType, clipperFillRule, solutionClosed);

            if (!started) started = true;
        }
    }
    
    public void ExecuteMany(IReadOnlyList<Vector2> subject, Paths64 clips, ShapeClipperClipType clipType, Paths64 solutionClosed)
    {
        var clipperClipType = clipType.ToClipperClipType();
        var clipperFillRule = FillRule.ToClipperFillRule();
        bool started = false;
        
        subject.ToPath64(path64ClipBuffer);
        
        foreach (var c in clips)
        {
            clipEngine.Clear();
            if(started)clipEngine.AddSubject(solutionClosed);
            else clipEngine.AddSubject(path64ClipBuffer);
            clipEngine.AddClip(c);
            clipEngine.Execute(clipperClipType, clipperFillRule, solutionClosed);

            if (!started) started = true;
        }
    }

    
    public void Execute(IReadOnlyList<IReadOnlyList<Vector2>> subject, IReadOnlyList<IReadOnlyList<Vector2>> clip, ShapeClipperClipType clipType, Paths64 solutionClosed)
    {
        clipEngine.Clear();
        paths64SubjectBuffer.PrepareBuffer(subject.Count);
        paths64ClipBuffer.PrepareBuffer(clip.Count);
        subject.ToPaths64(paths64SubjectBuffer.Buffer);
        clip.ToPaths64(paths64ClipBuffer.Buffer);
        clipEngine.AddSubject(paths64SubjectBuffer.Buffer);
        clipEngine.AddClip(paths64ClipBuffer.Buffer);
        clipEngine.Execute(clipType.ToClipperClipType(), FillRule.ToClipperFillRule(), solutionClosed);
    }
    
    public void Execute(IReadOnlyList<IReadOnlyList<Vector2>> subject, IReadOnlyList<Vector2> clip, ShapeClipperClipType clipType, Paths64 solutionClosed)
    {
        clipEngine.Clear();
        paths64SubjectBuffer.PrepareBuffer(subject.Count);
        subject.ToPaths64(paths64SubjectBuffer.Buffer);
        clipEngine.AddSubject(paths64SubjectBuffer.Buffer);
        clip.ToPath64(path64ClipBuffer);
        clipEngine.AddClip(path64ClipBuffer);
        clipEngine.Execute(clipType.ToClipperClipType(), FillRule.ToClipperFillRule(), solutionClosed);
    }
    
    public void Execute(IReadOnlyList<Vector2> subject, IReadOnlyList<IReadOnlyList<Vector2>> clip, ShapeClipperClipType clipType, Paths64 solutionClosed)
    {
        clipEngine.Clear();
        subject.ToPath64(path64SubjectBuffer);
        clipEngine.AddSubject(path64SubjectBuffer);
        
        paths64ClipBuffer.PrepareBuffer(clip.Count);
        clip.ToPaths64(paths64ClipBuffer.Buffer);
        clipEngine.AddClip(paths64ClipBuffer.Buffer);
        clipEngine.Execute(clipType.ToClipperClipType(), FillRule.ToClipperFillRule(), solutionClosed);
    }
    
    public void Execute(IReadOnlyList<Vector2> subject, IReadOnlyList<Vector2> clip, ShapeClipperClipType clipType, Paths64 solutionClosed)
    {
        clipEngine.Clear();
        subject.ToPath64(path64SubjectBuffer);
        clipEngine.AddSubject(path64SubjectBuffer);
        clip.ToPath64(path64ClipBuffer);
        clipEngine.AddClip(path64ClipBuffer);
        clipEngine.Execute(clipType.ToClipperClipType(), FillRule.ToClipperFillRule(), solutionClosed);
    }
    
    
    public void ExecuteMany(IReadOnlyList<IReadOnlyList<Vector2>> subject, IReadOnlyList<IReadOnlyList<Vector2>> clips, ShapeClipperClipType clipType, Paths64 solutionClosed)
    {
        var clipperClipType = clipType.ToClipperClipType();
        var clipperFillRule = FillRule.ToClipperFillRule();
        bool started = false;
        
        paths64SubjectBuffer.PrepareBuffer(subject.Count);
        subject.ToPaths64(paths64SubjectBuffer.Buffer);
        
        paths64ClipBuffer.PrepareBuffer(clips.Count);
        clips.ToPaths64(paths64ClipBuffer.Buffer);
        
        foreach (var c in paths64ClipBuffer.Buffer)
        {
            clipEngine.Clear();
            clipEngine.AddSubject(started ? solutionClosed : paths64SubjectBuffer.Buffer);
            clipEngine.AddClip(c);
            clipEngine.Execute(clipperClipType, clipperFillRule, solutionClosed);

            if (!started) started = true;
        }
    }
    
    public void ExecuteMany(IReadOnlyList<Vector2> subject, IReadOnlyList<IReadOnlyList<Vector2>> clips, ShapeClipperClipType clipType, Paths64 solutionClosed)
    {
        var clipperClipType = clipType.ToClipperClipType();
        var clipperFillRule = FillRule.ToClipperFillRule();
        bool started = false;
        
        subject.ToPath64(path64ClipBuffer);
        
        paths64ClipBuffer.PrepareBuffer(clips.Count);
        clips.ToPaths64(paths64ClipBuffer.Buffer);
        
        foreach (var c in paths64ClipBuffer.Buffer)
        {
            clipEngine.Clear();
            if(started)clipEngine.AddSubject(solutionClosed);
            else clipEngine.AddSubject(path64ClipBuffer);
            clipEngine.AddClip(c);
            clipEngine.Execute(clipperClipType, clipperFillRule, solutionClosed);

            if (!started) started = true;
        }
    }
    
    
    public void Execute(IReadOnlyList<IReadOnlyList<Vector2>> subject, IReadOnlyList<IReadOnlyList<Vector2>> clip, ShapeClipperClipType clipType, List<List<Vector2>> solutionClosed)
    {
        clipEngine.Clear();
        paths64SubjectBuffer.PrepareBuffer(subject.Count);
        paths64ClipBuffer.PrepareBuffer(clip.Count);
        subject.ToPaths64(paths64SubjectBuffer.Buffer);
        clip.ToPaths64(paths64ClipBuffer.Buffer);
        clipEngine.AddSubject(paths64SubjectBuffer.Buffer);
        clipEngine.AddClip(paths64ClipBuffer.Buffer);
        
        paths64SolutionBuffer.Clear();
        clipEngine.Execute(clipType.ToClipperClipType(), FillRule.ToClipperFillRule(), paths64SolutionBuffer);
        paths64SolutionBuffer.ToVector2Lists(solutionClosed);
    }
    
    public void Execute(IReadOnlyList<IReadOnlyList<Vector2>> subject, IReadOnlyList<Vector2> clip, ShapeClipperClipType clipType, List<List<Vector2>> solutionClosed)
    {
        clipEngine.Clear();
        paths64SubjectBuffer.PrepareBuffer(subject.Count);
        subject.ToPaths64(paths64SubjectBuffer.Buffer);
        clipEngine.AddSubject(paths64SubjectBuffer.Buffer);
        clip.ToPath64(path64ClipBuffer);
        clipEngine.AddClip(path64ClipBuffer);
        
        paths64SolutionBuffer.Clear();
        clipEngine.Execute(clipType.ToClipperClipType(), FillRule.ToClipperFillRule(), paths64SolutionBuffer);
        paths64SolutionBuffer.ToVector2Lists(solutionClosed);
    }
    
    public void Execute(IReadOnlyList<Vector2> subject, IReadOnlyList<IReadOnlyList<Vector2>> clip, ShapeClipperClipType clipType, List<List<Vector2>> solutionClosed)
    {
        clipEngine.Clear();
        subject.ToPath64(path64SubjectBuffer);
        clipEngine.AddSubject(path64SubjectBuffer);
        
        paths64ClipBuffer.PrepareBuffer(clip.Count);
        clip.ToPaths64(paths64ClipBuffer.Buffer);
        clipEngine.AddClip(paths64ClipBuffer.Buffer);
        
        paths64SolutionBuffer.Clear();
        clipEngine.Execute(clipType.ToClipperClipType(), FillRule.ToClipperFillRule(), paths64SolutionBuffer);
        paths64SolutionBuffer.ToVector2Lists(solutionClosed);
    }
    
    public void Execute(IReadOnlyList<Vector2> subject, IReadOnlyList<Vector2> clip, ShapeClipperClipType clipType, List<List<Vector2>> solutionClosed)
    {
        clipEngine.Clear();
        subject.ToPath64(path64SubjectBuffer);
        clipEngine.AddSubject(path64SubjectBuffer);
        clip.ToPath64(path64ClipBuffer);
        clipEngine.AddClip(path64ClipBuffer);
        
        paths64SolutionBuffer.Clear();
        clipEngine.Execute(clipType.ToClipperClipType(), FillRule.ToClipperFillRule(), paths64SolutionBuffer);
        paths64SolutionBuffer.ToVector2Lists(solutionClosed);
    }
    
    public void Execute(IReadOnlyList<Vector2> subject, IReadOnlyList<Vector2> clip, ShapeClipperClipType clipType, Polygons solutionClosed)
    {
        clipEngine.Clear();
        subject.ToPath64(path64SubjectBuffer);
        clipEngine.AddSubject(path64SubjectBuffer);
        clip.ToPath64(path64ClipBuffer);
        clipEngine.AddClip(path64ClipBuffer);
        
        paths64SolutionBuffer.Clear();
        clipEngine.Execute(clipType.ToClipperClipType(), FillRule.ToClipperFillRule(), paths64SolutionBuffer);
        paths64SolutionBuffer.ToPolygons(solutionClosed);
    }
    
    
    public void ExecuteMany(IReadOnlyList<IReadOnlyList<Vector2>> subject, IReadOnlyList<IReadOnlyList<Vector2>> clips, ShapeClipperClipType clipType, List<List<Vector2>> solutionClosed)
    {
        var clipperClipType = clipType.ToClipperClipType();
        var clipperFillRule = FillRule.ToClipperFillRule();
        bool started = false;
        
        paths64SubjectBuffer.PrepareBuffer(subject.Count);
        subject.ToPaths64(paths64SubjectBuffer.Buffer);
        
        paths64ClipBuffer.PrepareBuffer(clips.Count);
        clips.ToPaths64(paths64ClipBuffer.Buffer);
        
        paths64SolutionBuffer.Clear();
        
        foreach (var c in paths64ClipBuffer.Buffer)
        {
            clipEngine.Clear();
            clipEngine.AddSubject(started ? paths64SolutionBuffer : paths64SubjectBuffer.Buffer);
            clipEngine.AddClip(c);
            clipEngine.Execute(clipperClipType, clipperFillRule, paths64SolutionBuffer);

            if (!started) started = true;
        }
        paths64SolutionBuffer.ToVector2Lists(solutionClosed);
    }
    
    public void ExecuteMany(IReadOnlyList<Vector2> subject, IReadOnlyList<IReadOnlyList<Vector2>> clips, ShapeClipperClipType clipType, List<List<Vector2>> solutionClosed)
    {
        var clipperClipType = clipType.ToClipperClipType();
        var clipperFillRule = FillRule.ToClipperFillRule();
        bool started = false;
        
        subject.ToPath64(path64ClipBuffer);
        
        paths64ClipBuffer.PrepareBuffer(clips.Count);
        clips.ToPaths64(paths64ClipBuffer.Buffer);
        
        paths64SolutionBuffer.Clear();
        
        foreach (var c in paths64ClipBuffer.Buffer)
        {
            clipEngine.Clear();
            if(started)clipEngine.AddSubject(paths64SolutionBuffer);
            else clipEngine.AddSubject(path64ClipBuffer);
            clipEngine.AddClip(c);
            clipEngine.Execute(clipperClipType, clipperFillRule, paths64SolutionBuffer);

            if (!started) started = true;
        }
        paths64SolutionBuffer.ToVector2Lists(solutionClosed);
    }
    
    
    public void Execute(Paths64 subject, Paths64 clip, ShapeClipperClipType clipType, Paths64 solutionClosed, Paths64 solutionOpen)
    {
        clipEngine.Clear();
        clipEngine.AddSubject(subject);
        clipEngine.AddClip(clip);
        clipEngine.Execute(clipType.ToClipperClipType(), FillRule.ToClipperFillRule(), solutionClosed, solutionOpen);
    }
    
}