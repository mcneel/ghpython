using System;
using System.Collections;
using System.Collections.Generic;
using Grasshopper.Kernel.Types;
using Rhino.Collections;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace GhPython.DocReplacement
{
  public class CustomTable : IEnumerable<AttributedGeometry>
  {
    private readonly Dictionary<Guid, AttributedGeometry> _storage = new Dictionary<Guid, AttributedGeometry>();

    public bool Contains(Guid item)
    {
      return _storage.ContainsKey(item);
    }


    #region Members similar to Rhino.DocObjects.Tables.ObjectTable

    public Guid AddArc(Arc arc)
    {
      return AddArc(arc, null);
    }

    public Guid AddArc(Arc arc, ObjectAttributes attributes)
    {
      if (!arc.IsValid)
        return Guid.Empty;

      if (object.ReferenceEquals(attributes, null))
        attributes = new ObjectAttributes();

      Guid guid = Guid.NewGuid();
      _storage.Add(guid, new AttributedGeometry(new GH_Curve(new ArcCurve(arc)), attributes));
      return guid;
    }

    public Guid AddBrep(Brep brep)
    {
      return AddBrep(brep, null);
    }

    public Guid AddBrep(Brep brep, ObjectAttributes attributes)
    {
      return GenericAdd(new GH_Brep(brep), attributes);
    }

    public Guid AddCircle(Circle circle)
    {
      return AddCircle(circle, null);
    }

    public Guid AddCircle(Circle circle, ObjectAttributes attributes)
    {
      if (!circle.IsValid)
        return Guid.Empty;

      if (object.ReferenceEquals(attributes, null))
        attributes = new ObjectAttributes();

      Guid guid = Guid.NewGuid();
      _storage.Add(guid, new AttributedGeometry(new GH_Curve(new ArcCurve(circle)), attributes));
      return guid;
    }

    public Guid AddClippingPlane(Plane plane, double uMagnitude, double vMagnitude, Guid clippedViewportId)
    {
      throw NotSupportedExceptionHelp();
    }

    public Guid AddClippingPlane(Plane plane, double uMagnitude, double vMagnitude, IEnumerable<Guid> clippedViewportIds)
    {
      throw NotSupportedExceptionHelp();
    }

    public Guid AddClippingPlane(Plane plane, double uMagnitude, double vMagnitude, IEnumerable<Guid> clippedViewportIds,
                                 ObjectAttributes attributes)
    {
      throw new NotImplementedException("This call is not supported from a Grasshopper component");
    }

    public Guid AddCurve(Curve curve)
    {
      return GenericAdd(new GH_Curve(curve), null);
    }

    public Guid AddCurve(Curve curve, ObjectAttributes attributes)
    {
      return GenericAdd(new GH_Curve(curve), attributes);
    }

    public Guid AddEllipse(Ellipse ellipse)
    {
      return AddEllipse(ellipse, null);
    }

    public Guid AddEllipse(Ellipse ellipse, ObjectAttributes attributes)
    {
      if (!ellipse.Plane.IsValid || ellipse.Radius1 < Rhino.RhinoMath.ZeroTolerance || ellipse.Radius2 < Rhino.RhinoMath.ZeroTolerance)
        return Guid.Empty;

      if (object.ReferenceEquals(attributes, null))
        attributes = new ObjectAttributes();

      Guid guid = Guid.NewGuid();
      _storage.Add(guid, new AttributedGeometry(new GH_Curve(ellipse.ToNurbsCurve()), attributes));
      return guid;
    }

    public Guid AddExtrusion(Extrusion extrusion)
    {
      return AddExtrusion(extrusion, null);
    }

    public Guid AddExtrusion(Extrusion extrusion, ObjectAttributes attributes)
    {
      return GenericAdd(new GH_Surface(extrusion), attributes);
    }

    public Guid AddInstanceObject(int instanceDefinitionIndex, Transform instanceXform)
    {
      return AddInstanceObject(instanceDefinitionIndex, instanceXform, null);
    }

    public Guid AddInstanceObject(int instanceDefinitionIndex, Transform instanceXform, ObjectAttributes attributes)
    {
      throw NotSupportedExceptionHelp();
    }

    public Guid AddLeader(IEnumerable<Point3d> points)
    {
      throw NotSupportedExceptionHelp();
    }

    public Guid AddLeader(Plane plane, IEnumerable<Point2d> points)
    {
      throw NotSupportedExceptionHelp();
    }

    public Guid AddLeader(string text, IEnumerable<Point3d> points)
    {
      throw NotSupportedExceptionHelp();
    }

    public Guid AddLeader(Plane plane, IEnumerable<Point2d> points, ObjectAttributes attributes)
    {
      throw NotSupportedExceptionHelp();
    }

    public Guid AddLeader(string text, Plane plane, IEnumerable<Point2d> points)
    {
      throw NotSupportedExceptionHelp();
    }

    public Guid AddLeader(string text, Plane plane, IEnumerable<Point2d> points, ObjectAttributes attributes)
    {
      throw NotSupportedExceptionHelp();
    }

    public Guid AddLine(Line line)
    {
      return AddLine(line, null);
    }

    public Guid AddLine(Line line, ObjectAttributes attributes)
    {
      if (!line.IsValid)
        return Guid.Empty;

      if (object.ReferenceEquals(attributes, null))
        attributes = new ObjectAttributes();

      Guid guid = Guid.NewGuid();
      _storage.Add(guid, new AttributedGeometry(new GH_Curve(new LineCurve(line)), attributes));
      return guid;
    }

    public Guid AddLine(Point3d from, Point3d to)
    {
      return AddLine(new Line(from, to), null);
    }

    public Guid AddLine(Point3d from, Point3d to, ObjectAttributes attributes)
    {
      return AddLine(new Line(from, to), attributes);
    }

    public Guid AddLinearDimension(LinearDimension dimension)
    {
      return AddLinearDimension(dimension, null);
    }

    public Guid AddLinearDimension(LinearDimension dimension, ObjectAttributes attributes)
    {
      throw NotSupportedExceptionHelp();
    }

    public Guid AddMesh(Mesh mesh)
    {
      return AddMesh(mesh, null);
    }

    public Guid AddMesh(Mesh mesh, ObjectAttributes attributes)
    {
      return GenericAdd(new GH_Mesh(mesh), attributes);
    }

    public Guid AddPoint(Point3d point)
    {
      return AddPoint(point, null);
    }

    public Guid AddPoint(Point3f point)
    {
      return AddPoint(new Point3d(point));
    }

    public Guid AddPoint(Point3d point, ObjectAttributes attributes)
    {
      if (!point.IsValid)
        return Guid.Empty;

      Guid guid = Guid.NewGuid();
      if (object.ReferenceEquals(attributes, null))
        attributes = new ObjectAttributes();

      _storage.Add(guid, new AttributedGeometry(new GH_Point(point), attributes));
      return guid;
    }

    public Guid AddPoint(Point3f point, ObjectAttributes attributes)
    {
      return AddPoint(new Point3d(point), attributes);
    }

    public Guid AddPoint(double x, double y, double z)
    {
      return AddPoint(new Point3d(x, y, z), null);
    }

    public Guid AddPointCloud(IEnumerable<Point3d> points)
    {
      throw NotSupportedExceptionHelp();
    }

    public Guid AddPointCloud(PointCloud cloud)
    {
      throw NotSupportedExceptionHelp();
    }

    public Guid AddPointCloud(IEnumerable<Point3d> points, ObjectAttributes attributes)
    {
      return AddPointCloud(new PointCloud(points), attributes);
    }

    public Guid AddPointCloud(PointCloud cloud, ObjectAttributes attributes)
    {
      throw NotSupportedExceptionHelp();
    }

    public RhinoList<Guid> AddPoints(IEnumerable<Point3d> points)
    {
      return AddPoints(points, null);
    }

    public RhinoList<Guid> AddPoints(IEnumerable<Point3f> points)
    {
      return AddPoints(points, null);
    }

    public RhinoList<Guid> AddPoints(IEnumerable<Point3d> points, ObjectAttributes attributes)
    {
      RhinoList<Guid> list = new RhinoList<Guid>(InferLengthOrGuess(points));
      foreach (var p in points)
      {
        var id = AddPoint(p, attributes);
        if (!id.Equals(Guid.Empty))
          list.Add(id);
      }
      return list;
    }

    public RhinoList<Guid> AddPoints(IEnumerable<Point3f> points, ObjectAttributes attributes)
    {
      RhinoList<Guid> list = new RhinoList<Guid>(InferLengthOrGuess(points));
      foreach (var p in points)
      {
        var id = AddPoint(p, attributes);
        if (!id.Equals(Guid.Empty))
          list.Add(id);
      }
      return list;
    }

    public Guid AddPolyline(IEnumerable<Point3d> points)
    {
      return AddPolyline(points, null);
    }

    public Guid AddPolyline(IEnumerable<Point3d> points, ObjectAttributes attributes)
    {
      if (points == null)
        return Guid.Empty;

      return GenericAdd(new GH_Curve(new PolylineCurve(points)), attributes);
    }

    public Guid AddSphere(Sphere sphere)
    {
      return AddSphere(sphere, null);
    }

    public Guid AddSphere(Sphere sphere, ObjectAttributes attributes)
    {
      if (!sphere.IsValid)
        return Guid.Empty;

      return AddSurface(sphere.ToRevSurface(), attributes);
    }

    public Guid AddSurface(Surface surface)
    {
      return AddSurface(surface, null);
    }

    public Guid AddSurface(Surface surface, ObjectAttributes attributes)
    {
      if (surface == null)
        return Guid.Empty;

      return GenericAdd(new GH_Surface(surface), attributes);
    }

    public Guid AddText(Text3d text3d)
    {
      throw NotSupportedExceptionHelp();
    }

    public Guid AddText(Text3d text3d, ObjectAttributes attributes)
    {
      throw NotSupportedExceptionHelp();
    }

    public Guid AddText(string text, Plane plane, double height, string fontName, bool bold, bool italic)
    {
      throw NotSupportedExceptionHelp();
    }

    public Guid AddText(string text, Plane plane, double height, string fontName, bool bold, bool italic,
                        ObjectAttributes attributes)
    {
      throw NotSupportedExceptionHelp();
    }

    public Guid AddTextDot(TextDot dot)
    {
      throw NotSupportedExceptionHelp();
    }

    public Guid AddTextDot(string text, Point3d location)
    {
      throw NotSupportedExceptionHelp();
    }

    public Guid AddTextDot(TextDot dot, ObjectAttributes attributes)
    {
      throw NotSupportedExceptionHelp();
    }

    public Guid AddTextDot(string text, Point3d location, ObjectAttributes attributes)
    {
      throw NotSupportedExceptionHelp();
    }

    public bool Delete(Guid objectId, bool quiet)
    {
      bool success = _storage.Remove(objectId);

      if (!success && !quiet)
        throw new KeyNotFoundException("The Guid provided is not in the document");

      return success;
    }

    //public bool Delete(ObjRef objref, bool quiet)
    //{
    //    throw NotSupportedExceptionHelp();
    //}

    //public bool Delete(AttributedGeometry obj, bool quiet)
    //{
    //    throw NotSupportedExceptionHelp();
    //}

    public Guid Duplicate(Guid objectId)
    {
      if (!_storage.ContainsKey(objectId))
        return Guid.Empty;

      var attributedGeometry = _storage[objectId];

      ObjectAttributes attrDup = null;
      if (attributedGeometry.Attributes != null)
        attrDup = attributedGeometry.Attributes.Duplicate();

      return GenericAdd((IGH_GeometricGoo) attributedGeometry.GhGeometry.Duplicate(), attrDup);
    }

    //public Guid Duplicate(ObjRef objref)
    //{
    //    throw NotSupportedExceptionHelp();
    //}

    //public Guid Duplicate(AttributedGeometry obj)
    //{
    //    throw NotSupportedExceptionHelp();
    //}

    public AttributedGeometry Find(Guid objectId)
    {
      if (!_storage.ContainsKey(objectId))
        return default(AttributedGeometry);

      return _storage[objectId];
    }

    //public AttributedGeometry Find(uint runtimeSerialNumber)
    //{
    //    throw NotSupportedExceptionHelp();
    //}

    //public AttributedGeometry[] FindByFilter(ObjectEnumeratorSettings filter)
    //{
    //    throw new NotImplementedException();
    //}

    //public AttributedGeometry[] FindByGroup(int groupIndex)
    //{
    //    throw new NotImplementedException();
    //}

    //public AttributedGeometry[] FindByLayer(Layer layer)
    //{
    //    throw new NotImplementedException();
    //}

    //public AttributedGeometry[] FindByLayer(string layerName)
    //{
    //    throw new NotImplementedException();
    //}

    //public AttributedGeometry[] FindByObjectType(ObjectType typeFilter)
    //{
    //    throw new NotImplementedException();
    //}

    //public IEnumerable<AttributedGeometry> GetObjectList(ObjectEnumeratorSettings settings)
    //{
    //    throw new NotImplementedException();
    //}

    //public IEnumerable<AttributedGeometry> GetObjectList(ObjectType typeFilter)
    //{
    //    throw new NotImplementedException();
    //}

    //public IEnumerable<AttributedGeometry> GetSelectedObjects(bool includeLights, bool includeGrips)
    //{
    //    throw new NotImplementedException();
    //}

    //public AttributedGeometry GripUpdate(AttributedGeometry obj, bool deleteOriginal)
    //{
    //    throw new NotImplementedException();
    //}

    //public bool Hide(Guid objectId, bool ignoreLayerMode)
    //{
    //    throw new NotImplementedException();
    //}

    //public bool Hide(ObjRef objref, bool ignoreLayerMode)
    //{
    //    throw new NotImplementedException();
    //}

    //public bool Hide(AttributedGeometry obj, bool ignoreLayerMode)
    //{
    //    throw new NotImplementedException();
    //}

    //public bool Lock(Guid objectId, bool ignoreLayerMode)
    //{
    //    throw new NotImplementedException();
    //}

    //public bool Lock(ObjRef objref, bool ignoreLayerMode)
    //{
    //    throw new NotImplementedException();
    //}

    //public bool Lock(AttributedGeometry obj, bool ignoreLayerMode)
    //{
    //    throw new NotImplementedException();
    //}

    public bool ModifyAttributes(Guid objectId, ObjectAttributes newAttributes, bool quiet)
    {
      if (!_storage.ContainsKey(objectId))
        return false;

      if (newAttributes == null)
        newAttributes = new ObjectAttributes();
      _storage[objectId] = new AttributedGeometry(_storage[objectId].GhGeometry, newAttributes);
      return true;
    }

    //public bool ModifyAttributes(ObjRef objref, ObjectAttributes newAttributes, bool quiet)
    //{
    //    throw new NotImplementedException();
    //}

    //public bool ModifyAttributes(AttributedGeometry obj, ObjectAttributes newAttributes, bool quiet)
    //{
    //    throw new NotImplementedException();
    //}

    public int ObjectCount(ObjectEnumeratorSettings filter)
    {
      throw new NotImplementedException();
    }

    //public bool Replace(ObjRef objref, Arc arc)
    //{
    //    throw new NotImplementedException();
    //}

    //public bool Replace(ObjRef objref, Brep brep)
    //{
    //    throw new NotImplementedException();
    //}

    //public bool Replace(ObjRef objref, Circle circle)
    //{
    //    throw new NotImplementedException();
    //}

    //public bool Replace(ObjRef objref, Curve curve)
    //{
    //    throw new NotImplementedException();
    //}

    //public bool Replace(ObjRef objref, Line line)
    //{
    //    throw new NotImplementedException();
    //}

    //public bool Replace(ObjRef objref, Mesh mesh)
    //{
    //    throw new NotImplementedException();
    //}

    //public bool Replace(ObjRef objref, Point3d point)
    //{
    //    throw new NotImplementedException();
    //}

    //public bool Replace(ObjRef objref, Polyline polyline)
    //{
    //    throw new NotImplementedException();
    //}

    //public bool Replace(ObjRef objref, Surface surface)
    //{
    //    throw new NotImplementedException();
    //}

    //public bool Replace(ObjRef objref, TextDot dot)
    //{
    //    throw new NotImplementedException();
    //}

    //public bool Replace(ObjRef objref, TextEntity text)
    //{
    //    throw new NotImplementedException();
    //}

    public bool Replace(Guid guid, Arc arc)
    {
      if (!arc.IsValid || !Contains(guid))
        return false;

      _storage[guid] = new AttributedGeometry(new GH_Curve(new ArcCurve(arc)), _storage[guid].Attributes);
      return true;
    }

    public bool Replace(Guid guid, Brep brep)
    {
      if (!brep.IsValid || !Contains(guid))
        return false;

      _storage[guid] = new AttributedGeometry(new GH_Brep(brep), _storage[guid].Attributes);
      return true;
    }

    public bool Replace(Guid guid, Circle circle)
    {
      if (!circle.IsValid || !Contains(guid))
        return false;

      _storage[guid] = new AttributedGeometry(new GH_Curve(new ArcCurve(circle)), _storage[guid].Attributes);
      return true;
    }

    public bool Replace(Guid guid, Curve curve)
    {
      if (!curve.IsValid || !Contains(guid))
        return false;

      _storage[guid] = new AttributedGeometry(new GH_Curve(curve), _storage[guid].Attributes);
      return true;
    }

    public bool Replace(Guid guid, Line line)
    {
      if (!line.IsValid || !Contains(guid))
        return false;

      _storage[guid] = new AttributedGeometry(new GH_Curve(new LineCurve(line)), _storage[guid].Attributes);
      return true;
    }

    public bool Replace(Guid guid, Mesh mesh)
    {
      if (!mesh.IsValid || !Contains(guid))
        return false;

      _storage[guid] = new AttributedGeometry(new GH_Mesh(mesh), _storage[guid].Attributes);
      return true;
    }

    public bool Replace(Guid guid, Point3d point)
    {
      if (!point.IsValid || !Contains(guid))
        return false;

      _storage[guid] = new AttributedGeometry(new GH_Point(point), _storage[guid].Attributes);
      return true;
    }

    public bool Replace(Guid guid, Polyline polyline)
    {
      if (!polyline.IsValid || !Contains(guid))
        return false;

      _storage[guid] = new AttributedGeometry(new GH_Curve(new PolylineCurve(polyline)), _storage[guid].Attributes);
      return true;
    }

    public bool Replace(Guid guid, Surface surface)
    {
      if (!surface.IsValid || !Contains(guid))
        return false;

      _storage[guid] = new AttributedGeometry(new GH_Surface(surface), _storage[guid].Attributes);
      return true;
    }

    public bool Replace(Guid guid, TextDot dot)
    {
      throw NotSupportedExceptionHelp();
    }

    public bool Replace(Guid guid, TextEntity text)
    {
      throw NotSupportedExceptionHelp();
    }

    public bool Show(Guid objectId, bool ignoreLayerMode)
    {
      throw new NotImplementedException();
    }

    public bool Show(ObjRef objref, bool ignoreLayerMode)
    {
      throw new NotImplementedException();
    }

    public bool Show(AttributedGeometry obj, bool ignoreLayerMode)
    {
      throw new NotImplementedException();
    }

    public Guid Transform(Guid objectId, Transform xform, bool deleteOriginal)
    {
      if (!_storage.ContainsKey(objectId))
        return Guid.Empty;

      var obj = _storage[objectId];
      if (obj == null)
        return Guid.Empty;

      IGH_GeometricGoo newObj = deleteOriginal
                                  ? obj.GhGeometry.Transform(xform)
                                  : obj.GhGeometry.DuplicateGeometry().Transform(xform);

      if (newObj == null)
        return Guid.Empty;

      if (deleteOriginal)
      {
        obj.GhGeometry = newObj;
        _storage[objectId] = obj; // AttributedGeometry is a ValueType
        return objectId;
      }
      var newId = Guid.NewGuid();
      var attrClone = object.ReferenceEquals(obj.Attributes, null) ? null : obj.Attributes.Duplicate();
      _storage.Add(newId, new AttributedGeometry(newObj, attrClone));
      return newId;
    }

    //public Guid Transform(ObjRef objref, Transform xform, bool deleteOriginal)
    //{
    //    throw NotSupportedExceptionHelp();
    //}

    public bool Unlock(Guid objectId, bool ignoreLayerMode)
    {
      throw new NotImplementedException();
    }

    //public bool Unlock(ObjRef objref, bool ignoreLayerMode)
    //{
    //    throw new NotImplementedException();
    //}

    //public bool Unlock(AttributedGeometry obj, bool ignoreLayerMode)
    //{
    //    throw new NotImplementedException();
    //}

    public int UnselectAll()
    {
      throw new NotImplementedException();
    }

    public int UnselectAll(bool ignorePersistentSelections)
    {
      throw new NotImplementedException();
    }

    #endregion

    #region Enumerators

    public IEnumerator<AttributedGeometry> GetEnumerator()
    {
      return _storage.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion

    public Guid Add(GeometryBase geometry)
    {
      return __InternalAdd(geometry as dynamic);
    }

    // let python figure out the right overload to call. The double underscore
    // syntax hides these functions from the autocomplete in the python editor
    public Guid __InternalAdd(Arc arc)
    {
      return AddArc(arc);
    }

    public Guid __InternalAdd(Brep brep)
    {
      return AddBrep(brep);
    }

    public Guid __InternalAdd(Circle circle)
    {
      return AddCircle(circle);
    }

    public Guid __InternalAdd(Curve curve)
    {
      return AddCurve(curve);
    }

    public Guid __InternalAdd(Ellipse ellipse)
    {
      return AddEllipse(ellipse);
    }

    public Guid __InternalAdd(Extrusion extrusion)
    {
      return AddExtrusion(extrusion);
    }

    public Guid __InternalAdd(Line line)
    {
      return AddLine(line);
    }

    public Guid __InternalAdd(LinearDimension dimension)
    {
      return AddLinearDimension(dimension);
    }

    public Guid __InternalAdd(Mesh mesh)
    {
      return AddMesh(mesh);
    }

    public Guid __InternalAdd(Point point)
    {
      return AddPoint(point.Location);
    }

    public Guid __InternalAdd(Point3d point)
    {
      return AddPoint(point);
    }

    public Guid __InternalAdd(Point3f point)
    {
      return AddPoint(point);
    }

    public Guid __InternalAdd(PointCloud cloud)
    {
      return AddPointCloud(cloud);
    }

    public Guid __InternalAdd(Polyline polyline)
    {
      return AddPolyline(polyline);
    }

    public Guid __InternalAdd(Sphere sphere)
    {
      return AddSphere(sphere);
    }

    public Guid __InternalAdd(Surface surface)
    {
      return AddSurface(surface);
    }

    private void Add(AttributedGeometry item)
    {
      if (object.ReferenceEquals(item.Attributes, null))
        item.Attributes = new ObjectAttributes();

      _storage.Add(Guid.NewGuid(), item);
    }

    public void Clear()
    {
      if (_storage.Count != 0)
        _storage.Clear();
    }

    public bool Contains(AttributedGeometry item)
    {
      return _storage.ContainsValue(item);
    }

    public void CopyTo(AttributedGeometry[] array, int arrayIndex)
    {
      foreach (AttributedGeometry attr in _storage.Values)
      {
        array[arrayIndex++] = attr;
      }
    }

    public int Count
    {
      get { return _storage.Count; }
    }

    private Guid GenericAdd<T>(T obj, ObjectAttributes attributes)
      where T : IGH_GeometricGoo
    {
      if (obj == null || !obj.IsValid)
        return Guid.Empty;

      Guid guid = Guid.NewGuid();
      if (object.ReferenceEquals(attributes, null))
        attributes = new ObjectAttributes();

      _storage.Add(guid, new AttributedGeometry(obj, attributes));
      return guid;
    }

    public static NotSupportedException NotSupportedExceptionHelp()
    {
      return new NotSupportedException(
        "This call is not supported from within a Grasshopper component");
    }

    private const int _listInferStart = 4;

    private static int InferLengthOrGuess<T>(IEnumerable<T> points)
    {
      int inferredLength;

      if (points == null)
        inferredLength = 0;
      else
      {
        var col = points as ICollection<T>;
        inferredLength = col != null ? col.Count : _listInferStart;
      }
      return inferredLength;
    }

    public IEnumerable<IGH_GeometricGoo> GhGeometries
    {
      get
      {
        foreach (var v in _storage.Values)
        {
          yield return v.GhGeometry;
        }
      }
    }

    public IEnumerable Geometries
    {
      get
      {
        foreach (var v in _storage.Values)
        {
          yield return v.Geometry;
        }
      }
    }

    public IEnumerable<AttributedGeometry> AttributedGeometries
    {
      get { return _storage.Values; }
    }
  }
}
