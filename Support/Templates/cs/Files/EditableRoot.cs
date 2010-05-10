using System;
using Csla;

namespace Templates
{
  [Serializable]
  public class EditableRoot : BusinessBase<EditableRoot>
  {
    #region Business Methods

    // TODO: add your own fields, properties and methods

    // example with private backing field
    private static PropertyInfo<int> IdProperty =
      RegisterProperty(new PropertyInfo<int>("Id"));
    private int _Id = IdProperty.DefaultValue;
    public int Id
    {
      get { return GetProperty(IdProperty, _Id); }
      set { SetProperty(IdProperty, ref _Id, value); }
    }

    // example with managed backing field
    private static PropertyInfo<string> NameProperty =
      RegisterProperty(new PropertyInfo<string>("Name"));
    public string Name
    {
      get { return GetProperty(NameProperty); }
      set { SetProperty(NameProperty, value); }
    }

    #endregion

    #region Business Rules

    protected override void AddBusinessRules()
    {
      // TODO: add validation rules
      //BusinessRules.AddRule(new Rule(), IdProperty);
    }

    private static void AddObjectAuthorizationRules()
    {
      // TODO: add authorization rules
      //BusinessRules.AddRule(...);
    }

    #endregion

    #region Factory Methods

    public static EditableRoot NewEditableRoot()
    {
      return DataPortal.Create<EditableRoot>();
    }

    public static EditableRoot GetEditableRoot(int id)
    {
      return DataPortal.Fetch<EditableRoot>(
        new SingleCriteria<EditableRoot, int>(id));
    }

    public static void DeleteEditableRoot(int id)
    {
      DataPortal.Delete<EditableRoot>(new SingleCriteria<EditableRoot, int>(id));
    }

    private EditableRoot()
    { /* Require use of factory methods */ }

    #endregion

    #region Data Access

    [RunLocal]
    protected override void DataPortal_Create()
    {
      // TODO: load default values
      // omit this override if you have no defaults to set
      base.DataPortal_Create();
    }

    private void DataPortal_Fetch(SingleCriteria<EditableRoot, int> criteria)
    {
      // TODO: load values
    }

    [Transactional(TransactionalTypes.TransactionScope)]
    protected override void DataPortal_Insert()
    {
      // TODO: insert values
    }

    [Transactional(TransactionalTypes.TransactionScope)]
    protected override void DataPortal_Update()
    {
      // TODO: update values
    }

    [Transactional(TransactionalTypes.TransactionScope)]
    protected override void DataPortal_DeleteSelf()
    {
      DataPortal_Delete(new SingleCriteria<EditableRoot, int>(this.Id));
    }

    [Transactional(TransactionalTypes.TransactionScope)]
    private void DataPortal_Delete(SingleCriteria<EditableRoot, int> criteria)
    {
      // TODO: delete values
    }

    #endregion
  }
}
