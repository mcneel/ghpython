using System;
using System.Diagnostics;
using Grasshopper;
using Grasshopper.Kernel;

#if DEBUG
using System.Windows.Forms;
#endif

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
        GH_Document _doc;
        
        bool _initializationDone;
        bool _orphan;
        bool _locked;

        /// <summary>
        /// Do not use this constructor for initialization, but always use the Initialize() method, which will run only once.
        /// This constructor is called more times at startup for indexing the picture and some other external reasons.
        /// </summary>
        protected SafeComponent(string name, string abbreviation, string description, string category, string subCategory) :
            base(name, abbreviation, description, category, subCategory)
        {
            if (Instances.DocumentServer.DocumentCount > 0)
            {
                _doc = Instances.DocumentServer[0];

                CheckAndSetupActions();
            }
        }

        protected GH_Document Doc
        {
            get
            {
                CheckAndSetupActions();

                return _doc;
            }
        }

        public void CheckAndSetupActions()
        {
            if (_orphan)
            {
                _orphan = false;
                GC.ReRegisterForFinalize(this);
            }

            if (!_initializationDone)
            {
                if (_doc == null)
                {
                    _doc = OnPingDocument();

                    if (_doc == null) return;
                }

                _doc.ObjectsDeleted += GrasshopperObjectsDeleted;
                Instances.DocumentServer.DocumentRemoved += GrasshopperDocumentClosed;
                _doc.SolutionStart += AfterDocumentChanged;

                _initializationDone = true;
                Initialize();
            }
        }

        protected sealed override void SolveInstance(IGH_DataAccess DA)
        {
            CheckAndSetupActions();
            SafeSolveInstance(DA);
        }

        protected abstract void SafeSolveInstance(IGH_DataAccess DA);

        private void GrasshopperDocumentClosed(GH_DocumentServer sender, GH_Document doc)
        {
            if (doc != null && (_doc != null && doc.DocumentID == _doc.DocumentID))
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

            if (this.Locked != _locked)
            {
                _locked = this.Locked;
                OnLockedChanged(_locked);
            }
        }

        protected virtual void OnLockedChanged(bool nowIsLocked)
        {

        }

        private void DeregisterComponent()
        {
            if (_doc != null && _initializationDone)
            {
                _doc.ObjectsDeleted -= GrasshopperObjectsDeleted;

                if (Instances.DocumentServer != null)
                    Instances.DocumentServer.DocumentRemoved -= GrasshopperDocumentClosed;

               _doc.SolutionStart -= AfterDocumentChanged;
            }
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
            try
            {
                if (!_orphan)
                {
                    DeregisterComponent();
                    _orphan = true;
                    _initializationDone = false;

                    if (disposing)
                    {
                        GC.SuppressFinalize(this);
                    }
                }
            }
#if DEBUG
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
#else
               catch
               {
               }
#endif

        }

        /// <summary>
        /// The IDisposable implementation. You do not normally need to call this. The creator of this object will call it.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        ~SafeComponent()
        {
            Dispose(false);
        }
    }
}