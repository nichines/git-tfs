using System;
using System.Diagnostics;
using Microsoft.TeamFoundation.Server;
using Microsoft.TeamFoundation.VersionControl.Client;
using Sep.Git.Tfs.Core.TfsInterop;
using System.Collections.Generic;

namespace Sep.Git.Tfs.VsCommon
{
    class WrapperForVersionControlServer :WrapperFor<VersionControlServer>, IVersionControlServer
    {
        private readonly TfsApiBridge _bridge;
        private readonly VersionControlServer _versionControlServer;

        public WrapperForVersionControlServer(TfsApiBridge bridge, VersionControlServer versionControlServer) : base(versionControlServer)
        {
            _bridge = bridge;
            _versionControlServer = versionControlServer;
        }

        public IItem GetItem(int itemId, int changesetNumber)
        {
            return _bridge.Wrap<WrapperForItem, Item>(_versionControlServer.GetItem(itemId, changesetNumber));
        }

        public IItem GetItem(string itemPath, int changesetNumber)
        {
            return _bridge.Wrap<WrapperForItem, Item>(_versionControlServer.GetItem(itemPath, new ChangesetVersionSpec(changesetNumber)));
        }

        public IItem[] GetItems(string itemPath, int changesetNumber, TfsRecursionType recursionType)
        {
            var itemSet = _versionControlServer.GetItems(itemPath, new ChangesetVersionSpec(changesetNumber), _bridge.Convert<RecursionType>(recursionType));
            return _bridge.Wrap<WrapperForItem, Item>(itemSet.Items);
        }

        public IEnumerable<IChangeset> QueryHistory(string path, int version, int deletionId, 
            TfsRecursionType recursion, string user, int versionFrom, int versionTo, int maxCount, 
            bool includeChanges, bool slotMode, bool includeDownloadInfo)
        {
            var history = _versionControlServer.QueryHistory(path, new ChangesetVersionSpec(version), deletionId, _bridge.Convert<RecursionType>(recursion), user, new ChangesetVersionSpec(versionFrom), new ChangesetVersionSpec(versionTo), maxCount, includeChanges, slotMode, includeDownloadInfo);
            return _bridge.Wrap<WrapperForChangeset, Changeset>(history);
        }
    }

    class WrapperForChangeset : WrapperFor<Changeset>, IChangeset
    {
        private readonly TfsApiBridge _bridge;
        private readonly Changeset _changeset;

        public WrapperForChangeset(TfsApiBridge bridge, Changeset changeset) : base(changeset)
        {
            _bridge = bridge;
            _changeset = changeset;
        }

        public IChange [] Changes
        {
            get { return _bridge.Wrap<WrapperForChange, Change>(_changeset.Changes); }
        }

        public string Committer
        {
            get { return _changeset.Committer; }
        }

        public DateTime CreationDate
        {
            get { return _changeset.CreationDate; }
        }

        public string Comment
        {
            get { return _changeset.Comment; }
        }

        public int ChangesetId
        {
            get { return _changeset.ChangesetId; }
        }

        public IVersionControlServer VersionControlServer
        {
            get { return _bridge.Wrap<WrapperForVersionControlServer, VersionControlServer>(_changeset.VersionControlServer); }
        }
    }

    class WrapperForChange : WrapperFor<Change>, IChange
    {
        private readonly TfsApiBridge _bridge;
        private readonly Change _change;

        public WrapperForChange(TfsApiBridge bridge, Change change) : base(change)
        {
            _bridge = bridge;
            _change = change;
        }

        public TfsChangeType ChangeType
        {
            get { return _bridge.Convert<TfsChangeType>(_change.ChangeType); }
        }

        public IItem Item
        {
            get { return _bridge.Wrap<WrapperForItem, Item>(_change.Item); }
        }
    }

    class WrapperForItem : WrapperFor<Item>, IItem
    {
        private readonly TfsApiBridge _bridge;
        private readonly Item _item;

        public WrapperForItem(TfsApiBridge bridge, Item item) : base(item)
        {
            _bridge = bridge;
            _item = item;
        }

        public IVersionControlServer VersionControlServer
        {
            get { return _bridge.Wrap<WrapperForVersionControlServer, VersionControlServer>(_item.VersionControlServer); }
        }

        public int ChangesetId
        {
            get { return _item.ChangesetId; }
        }

        public string ServerItem
        {
            get { return _item.ServerItem; }
        }

        public decimal DeletionId
        {
            get { return _item.DeletionId; }
        }

        public TfsItemType ItemType
        {
            get { return _bridge.Convert<TfsItemType>(_item.ItemType); }
        }

        public int ItemId
        {
            get { return _item.ItemId; }
        }

        public void DownloadFile(string file)
        {
            _item.DownloadFile(file);
        }
    }

    class WrapperForIdentity : WrapperFor<Identity>, IIdentity
    {
        private readonly Identity _identity;

        public WrapperForIdentity(Identity identity) : base(identity)
        {
            Debug.Assert(identity != null, "wrapped property must not be null.");
            _identity = identity;
        }

        public string MailAddress
        {
            get { return _identity.MailAddress; }
        }

        public string DisplayName
        {
            get { return _identity.DisplayName; }
        }
    }

    class WrapperForShelveset : WrapperFor<Shelveset>, IShelveset
    {
        private readonly Shelveset _shelveset;
        private readonly TfsApiBridge _bridge;

        public WrapperForShelveset(TfsApiBridge bridge, Shelveset shelveset) : base(shelveset)
        {
            _shelveset = shelveset;
            _bridge = bridge;
        }

        public string Comment
        {
            get { return _shelveset.Comment; }
            set { _shelveset.Comment = value; }
        }

        public IWorkItemCheckinInfo[] WorkItemInfo
        {
            get { return _bridge.Wrap<WrapperForWorkItemCheckinInfo, WorkItemCheckinInfo>(_shelveset.WorkItemInfo); }
            set { _shelveset.WorkItemInfo = _bridge.Unwrap<WorkItemCheckinInfo>(value); }
        }
    }

    class WrapperForWorkItemCheckinInfo : WrapperFor<WorkItemCheckinInfo>, IWorkItemCheckinInfo
    {
        public WrapperForWorkItemCheckinInfo(WorkItemCheckinInfo workItemCheckinInfo) : base(workItemCheckinInfo)
        {
        }
    }

    class WrapperForPendingChange : WrapperFor<PendingChange>, IPendingChange
    {
        public WrapperForPendingChange(PendingChange pendingChange) : base(pendingChange)
        {
        }
    }

    class WrapperForWorkspace : WrapperFor<Workspace>, IWorkspace
    {
        private readonly TfsApiBridge _bridge;
        private readonly Workspace _workspace;

        public WrapperForWorkspace(TfsApiBridge bridge, Workspace workspace) : base(workspace)
        {
            _bridge = bridge;
            _workspace = workspace;
        }

        public IPendingChange [] GetPendingChanges()
        {
            return _bridge.Wrap<WrapperForPendingChange, PendingChange>(_workspace.GetPendingChanges());
        }

        public void Shelve(IShelveset shelveset, IPendingChange [] changes, TfsShelvingOptions options)
        {
            _workspace.Shelve(_bridge.Unwrap<Shelveset>(shelveset), _bridge.Unwrap<PendingChange>(changes), _bridge.Convert<ShelvingOptions>(options));
        }

        public int PendAdd(string path)
        {
            return _workspace.PendAdd(path);
        }

        public int PendEdit(string path)
        {
            return _workspace.PendEdit(path);
        }

        public int PendDelete(string path)
        {
            return _workspace.PendDelete(path);
        }

        public int PendRename(string pathFrom, string pathTo)
        {
            return _workspace.PendRename(pathFrom, pathTo);
        }

        public void ForceGetFile(string path, int changeset)
        {
            var item = new ItemSpec(path, RecursionType.None);
            _workspace.Get(new GetRequest(item, changeset), GetOptions.Overwrite | GetOptions.GetAll);
        }

        public string OwnerName
        {
            get { return _workspace.OwnerName; }
        }
    }
}
