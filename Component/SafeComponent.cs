using System;
using System.Diagnostics;
using Grasshopper;
using Grasshopper.Kernel;
using System.Windows.Forms;

namespace GhPython.Component
{
  /// <summary>
  /// This is an abstract class which automatically calls its own destructor when the user deletes it or
  /// closes the document. It handles most cases well, and relies on the GC for any other case. Please use the standard constructor 
  /// if you need to execute something also when the assembly loads, and the Initialize() method to setup any single component 
  /// when it first lands and executes on the canvas. Remember to call base.Dispose(disposing) if you override Dispose(bool).
  /// </summary>
  public abstract class SafeComponent : GH_Component, IDisposable
  {
    GH_Document m_doc;

    bool m_initializationDone;
    bool m_orphan;
    bool m_locked;
    bool m_afterDisposal;

    /// <summary>
    /// Do not use this constructor for initialization, but always use the Initialize() method, which will run only once.
    /// This constructor is called more times at startup for indexing the picture and some other external reasons.
    /// </summary>
    protected SafeComponent(string name, string abbreviation, string description, string category, string subCategory) :
      base(name, abbreviation, description, category, subCategory)
    {
      if (Instances.DocumentServer.DocumentCount > 0)
      {
        m_doc = Instances.DocumentServer[0];

        CheckIfSetupActionsAreNecessary();
      }
    }

    protected GH_Document Doc
    {
      get
      {
        CheckIfSetupActionsAreNecessary();
        return m_doc;
      }
    }

    public void CheckIfSetupActionsAreNecessary()
    {
      if (m_afterDisposal) return;

      if (m_orphan)
      {
        m_orphan = false;
        GC.ReRegisterForFinalize(this);
      }

      if (!m_initializationDone)
      {
        if (m_doc == null)
        {
          m_doc = OnPingDocument();

          if (m_doc == null) return;
        }

        m_doc.ObjectsDeleted += GrasshopperObjectsDeleted;
        Instances.DocumentServer.DocumentRemoved += GrasshopperDocumentClosed;
        m_doc.SolutionStart += AfterDocumentChanged;

        m_initializationDone = true;
        Initialize();
      }
    }

    protected sealed override void SolveInstance(IGH_DataAccess DA)
    {
      CheckIfSetupActionsAreNecessary();
      SafeSolveInstance(DA);
    }

    protected abstract void SafeSolveInstance(IGH_DataAccess DA);

    private void GrasshopperDocumentClosed(GH_DocumentServer sender, GH_Document doc)
    {
      if (doc != null && (m_doc != null && doc.DocumentID == m_doc.DocumentID))
      {
        Dispose();
      }
    }

    private void GrasshopperObjectsDeleted(object sender, GH_DocObjectEventArgs e)
    {
      if (e != null && e.Attributes != null)
      {
        for (int i = 0; i < e.ObjectCount; i++)
        {
          if (e.Attributes[i] != null && e.Attributes[i].InstanceGuid == this.InstanceGuid)
          {
            Dispose();
          }
          Debug.Assert(e.Attributes[i] != null, "e.Attributes[i] is null");
        }
      }
      Debug.Assert(e != null && e.Attributes != null, "e or e.Attributes is null");
    }

    void AfterDocumentChanged(object sender, GH_SolutionEventArgs args)
    {

      if (this.Locked != m_locked)
      {
        m_locked = this.Locked;
        OnLockedChanged(m_locked);
      }
    }

    protected virtual void OnLockedChanged(bool nowIsLocked)
    {
    }

    private void DeregisterComponent()
    {
      if (m_doc != null && m_initializationDone)
      {
        m_doc.ObjectsDeleted -= GrasshopperObjectsDeleted;

        if (Instances.DocumentServer != null)
          Instances.DocumentServer.DocumentRemoved -= GrasshopperDocumentClosed;

        m_doc.SolutionStart -= AfterDocumentChanged;
      }
    }

    public override void RemovedFromDocument(GH_Document document)
    {
      base.RemovedFromDocument(document);

      Dispose();
    }

    /// <summary>
    /// Initializes the component when it first executes and at no other earlier or later time. It runs once for each component
    /// </summary>
    protected virtual void Initialize()
    {
    }

    /// <summary>
    /// If you override this, be very sure you always call base.Dispose(disposing) or MyBase.Dispose(disposing) in
    /// Vb.Net from within your code.
    /// </summary>
    /// <param name="disposing">If disposing equals false, the method has been called by the 
    /// runtime from inside the finalizer and you should not reference other objects. Only unmanaged resources
    /// can be disposed.</param>
    protected virtual void Dispose(bool disposing)
    {
      m_afterDisposal = true;
      try
      {
        if (!m_orphan)
        {
          DeregisterComponent();
          m_orphan = true;
          m_initializationDone = false;

          if (disposing)
          {
            GC.SuppressFinalize(this);
          }
        }
      }
      catch (Exception ex)
      {
        GhPython.Forms.PythonScriptForm.LastHandleException(ex);
      }
    }

    /// <summary>
    /// The IDisposable implementation. You do not normally need to call this. The creator of this object will call it.
    /// </summary>
    public void Dispose()
    {
      m_afterDisposal = true;
      Dispose(true);
    }

    ~SafeComponent()
    {
      Dispose(false);
    }
  }
}