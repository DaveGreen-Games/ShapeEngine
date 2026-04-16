using System.Numerics;
using Clipper2Lib;
using ShapeEngine.Geometry.PolygonDef;

namespace ShapeEngine.ShapeClipper;

/// <summary>
/// Provides reusable helpers for performing boolean clipping operations on Clipper <see cref="Path64"/> and <see cref="Paths64"/> geometry, as well as engine polygon data represented by <see cref="Vector2"/> lists.
/// </summary>
/// <remarks>
/// This type wraps a reusable <see cref="Clipper64"/> instance and several internal buffers to reduce allocations across repeated clipping operations.
/// It is intended for sequential use and does not provide thread synchronization.
/// </remarks>
public class ShapeClipper64
{
    #region Helper
    
    private readonly Paths64PooledBuffer paths64SubjectBuffer = new();
    private readonly Paths64PooledBuffer paths64ClipBuffer = new();
    private readonly Path64 path64SubjectBuffer = new();
    private readonly Path64 path64ClipBuffer = new();
    private readonly Paths64 paths64SolutionBuffer = new();
    
    #endregion
    
    #region Members
    private readonly Clipper64 clipEngine;
    
    /// <summary>
    /// Gets or sets the fill rule used when evaluating subject and clip geometry.
    /// </summary>
    public ShapeClipperFillRule FillRule = ShapeClipperFillRule.NonZero;

    /// <summary>
    /// Gets or sets whether collinear edges should be preserved in generated solutions.
    /// </summary>
    public bool PreserveCollinear
    {
        get => clipEngine.PreserveCollinear;
        set => clipEngine.PreserveCollinear = value;
    }

    /// <summary>
    /// Gets or sets whether generated solution paths should use reversed winding.
    /// </summary>
    public bool ReverseSolution
    {
        get => clipEngine.ReverseSolution;
        set => clipEngine.ReverseSolution = value;
    }

    #endregion
    
    #region Constructor
    
    /// <summary>
    /// Initializes a new <see cref="ShapeClipper64"/> with the specified clipping engine settings.
    /// </summary>
    /// <param name="preserveCollinear">Whether collinear edges should be preserved in generated solutions.</param>
    /// <param name="reverseSolution">Whether generated solution paths should use reversed winding.</param>
    public ShapeClipper64(bool preserveCollinear = true, bool reverseSolution = false)
    {
        clipEngine = new();
        clipEngine.PreserveCollinear = preserveCollinear;
        clipEngine.ReverseSolution = reverseSolution;
    }
    
    #endregion

    #region Execute Path 64 Input / Path 64 Solution
    
    /// <summary>
    /// Executes a boolean clipping operation using multiple subject paths and multiple clip paths.
    /// </summary>
    /// <param name="subject">The closed subject paths.</param>
    /// <param name="clip">The closed clip paths.</param>
    /// <param name="clipType">The clipping operation to perform.</param>
    /// <param name="solutionClosed">The destination collection for the resulting closed solution paths.</param>
    public void Execute(Paths64 subject, Paths64 clip, ShapeClipperClipType clipType, Paths64 solutionClosed)
    {
        clipEngine.Clear();
        clipEngine.AddSubject(subject);
        clipEngine.AddClip(clip);
        clipEngine.Execute(clipType.ToClipperClipType(), FillRule.ToClipperFillRule(), solutionClosed);
    }
    
    /// <summary>
    /// Executes a boolean clipping operation using multiple subject paths and a single clip path.
    /// </summary>
    /// <param name="subject">The closed subject paths.</param>
    /// <param name="clip">The closed clip path.</param>
    /// <param name="clipType">The clipping operation to perform.</param>
    /// <param name="solutionClosed">The destination collection for the resulting closed solution paths.</param>
    public void Execute(Paths64 subject, Path64 clip, ShapeClipperClipType clipType, Paths64 solutionClosed)
    {
        clipEngine.Clear();
        clipEngine.AddSubject(subject);
        clipEngine.AddClip(clip);
        clipEngine.Execute(clipType.ToClipperClipType(), FillRule.ToClipperFillRule(), solutionClosed);
    }
    
    /// <summary>
    /// Executes a boolean clipping operation using a single subject path and multiple clip paths.
    /// </summary>
    /// <param name="subject">The closed subject path.</param>
    /// <param name="clip">The closed clip paths.</param>
    /// <param name="clipType">The clipping operation to perform.</param>
    /// <param name="solutionClosed">The destination collection for the resulting closed solution paths.</param>
    public void Execute(Path64 subject, Paths64 clip, ShapeClipperClipType clipType, Paths64 solutionClosed)
    {
        clipEngine.Clear();
        clipEngine.AddSubject(subject);
        clipEngine.AddClip(clip);
        clipEngine.Execute(clipType.ToClipperClipType(), FillRule.ToClipperFillRule(), solutionClosed);
    }
    
    /// <summary>
    /// Executes a boolean clipping operation using a single subject path and a single clip path.
    /// </summary>
    /// <param name="subject">The closed subject path.</param>
    /// <param name="clip">The closed clip path.</param>
    /// <param name="clipType">The clipping operation to perform.</param>
    /// <param name="solutionClosed">The destination collection for the resulting closed solution paths.</param>
    public void Execute(Path64 subject, Path64 clip, ShapeClipperClipType clipType, Paths64 solutionClosed)
    {
        clipEngine.Clear();
        clipEngine.AddSubject(subject);
        clipEngine.AddClip(clip);
        clipEngine.Execute(clipType.ToClipperClipType(), FillRule.ToClipperFillRule(), solutionClosed);
    }
    
    #endregion
    
    #region Execute Many Path 64 Input / Path 64 Solution
    
    /// <summary>
    /// Applies the same clipping operation sequentially to a set of clip paths against multiple subject paths.
    /// </summary>
    /// <param name="subject">The initial closed subject paths.</param>
    /// <param name="clips">The clip paths to apply one after another.</param>
    /// <param name="clipType">The clipping operation to perform for each step.</param>
    /// <param name="solutionClosed">The destination collection for the accumulated closed solution paths.</param>
    /// <remarks>
    /// Each clip path is applied to the result of the previous step. If <paramref name="clips"/> is empty, this method performs no clipping.
    /// </remarks>
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
    
    /// <summary>
    /// Applies the same clipping operation sequentially to a set of clip paths against a single subject path.
    /// </summary>
    /// <param name="subject">The initial closed subject path.</param>
    /// <param name="clips">The clip paths to apply one after another.</param>
    /// <param name="clipType">The clipping operation to perform for each step.</param>
    /// <param name="solutionClosed">The destination collection for the accumulated closed solution paths.</param>
    /// <remarks>
    /// Each clip path is applied to the result of the previous step. If <paramref name="clips"/> is empty, this method performs no clipping.
    /// </remarks>
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
    
    /// <summary>
    /// Applies the same clipping operation sequentially to a set of clip paths against a subject polygon expressed as <see cref="Vector2"/> vertices.
    /// </summary>
    /// <param name="subject">The initial closed subject polygon.</param>
    /// <param name="clips">The clip paths to apply one after another.</param>
    /// <param name="clipType">The clipping operation to perform for each step.</param>
    /// <param name="solutionClosed">The destination collection for the accumulated closed solution paths.</param>
    /// <remarks>
    /// The subject polygon is converted to a temporary <see cref="Path64"/> once before the sequential clipping loop begins.
    /// </remarks>
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

    #endregion

    #region Execute IReadOnlyLists Input / Path 64 Solution
    
    /// <summary>
    /// Executes a boolean clipping operation using subject and clip geometry expressed as collections of polygons.
    /// </summary>
    /// <param name="subject">The subject polygons.</param>
    /// <param name="clip">The clip polygons.</param>
    /// <param name="clipType">The clipping operation to perform.</param>
    /// <param name="solutionClosed">The destination collection for the resulting closed solution paths.</param>
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
    
    /// <summary>
    /// Executes a boolean clipping operation using subject polygons and a single clip polygon expressed as <see cref="Vector2"/> vertices.
    /// </summary>
    /// <param name="subject">The subject polygons.</param>
    /// <param name="clip">The clip polygon.</param>
    /// <param name="clipType">The clipping operation to perform.</param>
    /// <param name="solutionClosed">The destination collection for the resulting closed solution paths.</param>
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
    
    /// <summary>
    /// Executes a boolean clipping operation using a single subject polygon and multiple clip polygons expressed as <see cref="Vector2"/> vertices.
    /// </summary>
    /// <param name="subject">The subject polygon.</param>
    /// <param name="clip">The clip polygons.</param>
    /// <param name="clipType">The clipping operation to perform.</param>
    /// <param name="solutionClosed">The destination collection for the resulting closed solution paths.</param>
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
    
    /// <summary>
    /// Executes a boolean clipping operation using a single subject polygon and a single clip polygon expressed as <see cref="Vector2"/> vertices.
    /// </summary>
    /// <param name="subject">The subject polygon.</param>
    /// <param name="clip">The clip polygon.</param>
    /// <param name="clipType">The clipping operation to perform.</param>
    /// <param name="solutionClosed">The destination collection for the resulting closed solution paths.</param>
    public void Execute(IReadOnlyList<Vector2> subject, IReadOnlyList<Vector2> clip, ShapeClipperClipType clipType, Paths64 solutionClosed)
    {
        clipEngine.Clear();
        subject.ToPath64(path64SubjectBuffer);
        clipEngine.AddSubject(path64SubjectBuffer);
        clip.ToPath64(path64ClipBuffer);
        clipEngine.AddClip(path64ClipBuffer);
        clipEngine.Execute(clipType.ToClipperClipType(), FillRule.ToClipperFillRule(), solutionClosed);
    }
    
    #endregion

    #region Execute Many IReadOnlyLists Input / Path 64 Solution
    
    /// <summary>
    /// Applies the same clipping operation sequentially to multiple clip polygons against subject polygons expressed as <see cref="Vector2"/> vertices.
    /// </summary>
    /// <param name="subject">The initial subject polygons.</param>
    /// <param name="clips">The clip polygons to apply one after another.</param>
    /// <param name="clipType">The clipping operation to perform for each step.</param>
    /// <param name="solutionClosed">The destination collection for the accumulated closed solution paths.</param>
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
    
    /// <summary>
    /// Applies the same clipping operation sequentially to multiple clip polygons against a single subject polygon expressed as <see cref="Vector2"/> vertices.
    /// </summary>
    /// <param name="subject">The initial subject polygon.</param>
    /// <param name="clips">The clip polygons to apply one after another.</param>
    /// <param name="clipType">The clipping operation to perform for each step.</param>
    /// <param name="solutionClosed">The destination collection for the accumulated closed solution paths.</param>
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
    
    #endregion
    
    #region Execute IReadOnlyLists Input / Vector2 List Solution
    
    /// <summary>
    /// Executes a boolean clipping operation using subject and clip polygons expressed as <see cref="Vector2"/> vertex lists and converts the solution to nested <see cref="Vector2"/> lists.
    /// </summary>
    /// <param name="subject">The subject polygons.</param>
    /// <param name="clip">The clip polygons.</param>
    /// <param name="clipType">The clipping operation to perform.</param>
    /// <param name="solutionClosed">The destination collection for the resulting closed solution polygons.</param>
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
    
    /// <summary>
    /// Executes a boolean clipping operation using subject polygons and a single clip polygon, then converts the solution to nested <see cref="Vector2"/> lists.
    /// </summary>
    /// <param name="subject">The subject polygons.</param>
    /// <param name="clip">The clip polygon.</param>
    /// <param name="clipType">The clipping operation to perform.</param>
    /// <param name="solutionClosed">The destination collection for the resulting closed solution polygons.</param>
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
    
    /// <summary>
    /// Executes a boolean clipping operation using a single subject polygon and multiple clip polygons, then converts the solution to nested <see cref="Vector2"/> lists.
    /// </summary>
    /// <param name="subject">The subject polygon.</param>
    /// <param name="clip">The clip polygons.</param>
    /// <param name="clipType">The clipping operation to perform.</param>
    /// <param name="solutionClosed">The destination collection for the resulting closed solution polygons.</param>
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
    
    /// <summary>
    /// Executes a boolean clipping operation using a single subject polygon and a single clip polygon, then converts the solution to nested <see cref="Vector2"/> lists.
    /// </summary>
    /// <param name="subject">The subject polygon.</param>
    /// <param name="clip">The clip polygon.</param>
    /// <param name="clipType">The clipping operation to perform.</param>
    /// <param name="solutionClosed">The destination collection for the resulting closed solution polygons.</param>
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
    
    /// <summary>
    /// Executes a boolean clipping operation using a single subject polygon and a single clip polygon, then converts the solution to a <see cref="Polygons"/> collection.
    /// </summary>
    /// <param name="subject">The subject polygon.</param>
    /// <param name="clip">The clip polygon.</param>
    /// <param name="clipType">The clipping operation to perform.</param>
    /// <param name="solutionClosed">The destination collection for the resulting closed polygons.</param>
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
    
    #endregion
    
    #region Execute Many IReadOnlyLists Input / Vector2 List Solution
    
    /// <summary>
    /// Applies the same clipping operation sequentially to multiple clip polygons against subject polygons, then converts the final solution to nested <see cref="Vector2"/> lists.
    /// </summary>
    /// <param name="subject">The initial subject polygons.</param>
    /// <param name="clips">The clip polygons to apply one after another.</param>
    /// <param name="clipType">The clipping operation to perform for each step.</param>
    /// <param name="solutionClosed">The destination collection for the final closed solution polygons.</param>
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
    
    /// <summary>
    /// Applies the same clipping operation sequentially to multiple clip polygons against a single subject polygon, then converts the final solution to nested <see cref="Vector2"/> lists.
    /// </summary>
    /// <param name="subject">The initial subject polygon.</param>
    /// <param name="clips">The clip polygons to apply one after another.</param>
    /// <param name="clipType">The clipping operation to perform for each step.</param>
    /// <param name="solutionClosed">The destination collection for the final closed solution polygons.</param>
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
    
    #endregion
    
    
    /// <summary>
    /// Executes a boolean clipping operation and collects both closed and open solution paths.
    /// </summary>
    /// <param name="subject">The closed subject paths.</param>
    /// <param name="clip">The closed clip paths.</param>
    /// <param name="clipType">The clipping operation to perform.</param>
    /// <param name="solutionClosed">The destination collection for the resulting closed solution paths.</param>
    /// <param name="solutionOpen">The destination collection for the resulting open solution paths.</param>
    public void Execute(Paths64 subject, Paths64 clip, ShapeClipperClipType clipType, Paths64 solutionClosed, Paths64 solutionOpen)
    {
        clipEngine.Clear();
        clipEngine.AddSubject(subject);
        clipEngine.AddClip(clip);
        clipEngine.Execute(clipType.ToClipperClipType(), FillRule.ToClipperFillRule(), solutionClosed, solutionOpen);
    }
    
}