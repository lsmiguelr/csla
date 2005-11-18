Imports System.Threading
Imports System.Reflection
Imports System.Runtime.Remoting
Imports System.Runtime.Remoting.Channels
Imports System.Runtime.Remoting.Channels.Http

''' <summary>
''' This is the client-side DataPortal as described in
''' Chapter 5.
''' </summary>
Public NotInheritable Class DataPortal

#Region " Constructors "

  Private Sub New()

  End Sub

#End Region

#Region " DataPortal events "

  ''' <summary>
  ''' Raised by DataPortal prior to calling the 
  ''' requested server-side DataPortal method.
  ''' </summary>
  <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")> _
  Public Shared Event DataPortalInvoke As Action(Of DataPortalEventArgs)
  ''' <summary>
  ''' Raised by DataPortal after the requested 
  ''' server-side DataPortal method call is complete.
  ''' </summary>
  <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")> _
  Public Shared Event DataPortalInvokeComplete As Action(Of DataPortalEventArgs)

  Private Shared Sub OnDataPortalInvoke(ByVal e As DataPortalEventArgs)

    RaiseEvent DataPortalInvoke(e)

  End Sub

  Private Shared Sub OnDataPortalInvokeComplete(ByVal e As DataPortalEventArgs)

    RaiseEvent DataPortalInvokeComplete(e)

  End Sub

#End Region

#Region " Data Access methods "

  ''' <summary>
  ''' Called by a factory method in a business class to create 
  ''' a new object, which is loaded with default
  ''' values from the database.
  ''' </summary>
  ''' <typeparam name="T">Specific type of the business object.</typeparam>
  ''' <param name="Criteria">Object-specific criteria.</param>
  ''' <returns>A new object, populated with default values.</returns>
  Public Shared Function Create(Of T)(ByVal criteria As Object) As T
    Return DirectCast(Create(GetType(T), criteria), T)
  End Function

  ''' <summary>
  ''' Called by a factory method in a business class to create 
  ''' a new object, which is loaded with default
  ''' values from the database.
  ''' </summary>
  ''' <typeparam name="T">Specific type of the business object.</typeparam>
  ''' <returns>A new object, populated with default values.</returns>
  Public Shared Function Create(Of T)() As T
    Return DirectCast(Create(GetType(T), Nothing), T)
  End Function

  ''' <summary>
  ''' Called by a factory method in a business class to create 
  ''' a new object, which is loaded with default
  ''' values from the database.
  ''' </summary>
  ''' <param name="Criteria">Object-specific criteria.</param>
  ''' <returns>A new object, populated with default values.</returns>
  <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2223:MembersShouldDifferByMoreThanReturnType")> _
  <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")> _
  Public Shared Function Create(ByVal criteria As Object) As Object

    Return Create(GetObjectType(criteria), criteria)

  End Function

  Private Shared Function Create(ByVal objectType As Type, ByVal criteria As Object) As Object

    Dim result As Server.DataPortalResult

    Dim method As MethodInfo = _
      MethodCaller.GetMethod(objectType, "DataPortal_Create", criteria)

    Dim portal As DataPortalClient.IDataPortalProxy
    portal = GetDataPortalProxy(RunLocal(method))

    Dim dpContext As New Server.DataPortalContext(GetPrincipal, portal.IsServerRemote)

    OnDataPortalInvoke(New DataPortalEventArgs(dpContext))

    Try
      result = CType(portal.Create( _
        objectType, criteria, dpContext), Server.DataPortalResult)

    Catch ex As Server.DataPortalException
      result = ex.Result
      If portal.IsServerRemote Then
        RestoreContext(result)
      End If
      Throw New DataPortalException("DataPortal.Create " & _
        My.Resources.Failed, ex.InnerException, result.ReturnObject)
    End Try

    If portal.IsServerRemote Then
      RestoreContext(result)
    End If

    OnDataPortalInvokeComplete(New DataPortalEventArgs(dpContext))

    Return result.ReturnObject

  End Function

  ''' <summary>
  ''' Called by a factory method in a business class to retrieve
  ''' an object, which is loaded with values from the database.
  ''' </summary>
  ''' <typeparam name="T">Specific type of the business object.</typeparam>
  ''' <param name="Criteria">Object-specific criteria.</param>
  ''' <returns>An object populated with values from the database.</returns>
  <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2223:MembersShouldDifferByMoreThanReturnType")> _
  <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")> _
  <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId:="Csla.DataPortalException.#ctor(System.String,System.Exception,System.Object)")> _
  Public Shared Function Fetch(Of T)(ByVal criteria As Object) As T

    Return DirectCast(Fetch(criteria), T)

  End Function

  ''' <summary>
  ''' Called by a factory method in a business class to retrieve
  ''' an object, which is loaded with values from the database.
  ''' </summary>
  ''' <param name="Criteria">Object-specific criteria.</param>
  ''' <returns>An object populated with values from the database.</returns>
  Public Shared Function Fetch(ByVal criteria As Object) As Object

    Dim result As Server.DataPortalResult

    Dim method As MethodInfo = _
      MethodCaller.GetMethod(GetObjectType(criteria), "DataPortal_Fetch", criteria)

    Dim portal As DataPortalClient.IDataPortalProxy
    portal = GetDataPortalProxy(RunLocal(method))

    Dim dpContext As New Server.DataPortalContext(GetPrincipal, portal.IsServerRemote)

    OnDataPortalInvoke(New DataPortalEventArgs(dpContext))

    Try
      result = CType(portal.Fetch(criteria, dpContext), Server.DataPortalResult)

    Catch ex As Server.DataPortalException
      result = ex.Result
      If portal.IsServerRemote Then
        RestoreContext(result)
      End If
      Throw New DataPortalException("DataPortal.Fetch " & _
        My.Resources.Failed, ex.InnerException, result.ReturnObject)
    End Try

    If portal.IsServerRemote Then
      RestoreContext(result)
    End If

    OnDataPortalInvokeComplete(New DataPortalEventArgs(dpContext))

    Return result.ReturnObject

  End Function

  ''' <summary>
  ''' Called to execute a Command object on the server.
  ''' </summary>
  ''' <remarks>
  ''' <para>
  ''' To be a Command object, the object must inherit from
  ''' <see cref="CommandBase">CommandBase</see>.
  ''' </para><para>
  ''' Note that this method returns a reference to the updated business object.
  ''' If the server-side DataPortal is running remotely, this will be a new and
  ''' different object from the original, and all object references MUST be updated
  ''' to use this new object.
  ''' </para><para>
  ''' On the server, the Command object's DataPortal_Execute() method will
  ''' be invoked. Write any server-side code in that method.
  ''' </para>
  ''' </remarks>
  ''' <typeparam name="T">Specific type of the Command object.</typeparam>
  ''' <param name="obj">A reference to the Command object to be executed.</param>
  ''' <returns>A reference to the updated Command object.</returns>
  <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")> _
  <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", _
    MessageId:="Csla.DataPortalException.#ctor(System.String,System.Exception,System.Object)")> _
  Public Shared Function Execute(Of T As CommandBase)(ByVal obj As T) As T

    Return DirectCast(Update(CObj(obj)), T)

  End Function

  ''' <summary>
  ''' Called to execute a Command object on the server.
  ''' </summary>
  ''' <remarks>
  ''' <para>
  ''' Note that this method returns a reference to the updated business object.
  ''' If the server-side DataPortal is running remotely, this will be a new and
  ''' different object from the original, and all object references MUST be updated
  ''' to use this new object.
  ''' </para><para>
  ''' On the server, the Command object's DataPortal_Execute() method will
  ''' be invoked. Write any server-side code in that method.
  ''' </para>
  ''' </remarks>
  ''' <param name="obj">A reference to the Command object to be executed.</param>
  ''' <returns>A reference to the updated Command object.</returns>
  <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")> _
  <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId:="Csla.DataPortalException.#ctor(System.String,System.Exception,System.Object)")> _
  Public Shared Function Execute(ByVal obj As CommandBase) As CommandBase

    Return DirectCast(Update(obj), CommandBase)

  End Function

  ''' <summary>
  ''' Called by the <see cref="M:Csla.BusinessBase.Save" /> method to
  ''' insert, update or delete an object in the database.
  ''' </summary>
  ''' <remarks>
  ''' Note that this method returns a reference to the updated business object.
  ''' If the server-side DataPortal is running remotely, this will be a new and
  ''' different object from the original, and all object references MUST be updated
  ''' to use this new object.
  ''' </remarks>
  ''' <typeparam name="T">Specific type of the business object.</typeparam>
  ''' <param name="obj">A reference to the business object to be updated.</param>
  ''' <returns>A reference to the updated business object.</returns>
  <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")> _
  <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId:="Csla.DataPortalException.#ctor(System.String,System.Exception,System.Object)")> _
  Public Shared Function Update(Of T)(ByVal obj As T) As T

    Return DirectCast(Update(CObj(obj)), T)

  End Function

  ''' <summary>
  ''' Called by the <see cref="M:Csla.BusinessBase.Save" /> method to
  ''' insert, update or delete an object in the database.
  ''' </summary>
  ''' <remarks>
  ''' Note that this method returns a reference to the updated business object.
  ''' If the server-side DataPortal is running remotely, this will be a new and
  ''' different object from the original, and all object references MUST be updated
  ''' to use this new object.
  ''' </remarks>
  ''' <param name="obj">A reference to the business object to be updated.</param>
  ''' <returns>A reference to the updated business object.</returns>
  Public Shared Function Update(ByVal obj As Object) As Object

    Dim result As Server.DataPortalResult

    Dim method As MethodInfo
    If TypeOf obj Is CommandBase Then
      method = _
        MethodCaller.GetMethod(obj.GetType, "DataPortal_Execute")

    Else
      method = MethodCaller.GetMethod(obj.GetType, "DataPortal_Update")
    End If

    Dim portal As DataPortalClient.IDataPortalProxy
    portal = GetDataPortalProxy(RunLocal(method))

    Dim dpContext As New Server.DataPortalContext(GetPrincipal, portal.IsServerRemote)

    OnDataPortalInvoke(New DataPortalEventArgs(dpContext))

    Try
      result = CType(portal.Update(obj, dpContext), Server.DataPortalResult)

    Catch ex As Server.DataPortalException
      result = ex.Result
      If portal.IsServerRemote Then
        RestoreContext(result)
      End If
      Throw New DataPortalException("DataPortal.Update " & _
        My.Resources.Failed, ex.InnerException, result.ReturnObject)
    End Try

    If portal.IsServerRemote Then
      RestoreContext(result)
    End If

    OnDataPortalInvokeComplete(New DataPortalEventArgs(dpContext))

    Return result.ReturnObject

  End Function

  ''' <summary>
  ''' Called by a <c>Shared</c> method in the business class to cause
  ''' immediate deletion of a specific object from the database.
  ''' </summary>
  ''' <param name="criteria">Object-specific criteria.</param>
  <System.Diagnostics.CodeAnalysis.SuppressMessage( _
    "Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", _
    MessageId:="Csla.DataPortalException.#ctor(System.String,System.Exception,System.Object)")> _
  Public Shared Sub Delete(ByVal criteria As Object)

    Dim result As Server.DataPortalResult

    Dim method As MethodInfo = _
      MethodCaller.GetMethod(GetObjectType(criteria), "DataPortal_Delete", criteria)

    Dim portal As DataPortalClient.IDataPortalProxy
    portal = GetDataPortalProxy(RunLocal(method))

    Dim dpContext As New Server.DataPortalContext(GetPrincipal, portal.IsServerRemote)

    OnDataPortalInvoke(New DataPortalEventArgs(dpContext))

    Try
      result = CType(portal.Delete(criteria, dpContext), Server.DataPortalResult)

    Catch ex As Server.DataPortalException
      result = ex.Result
      If portal.IsServerRemote Then
        RestoreContext(result)
      End If
      Throw New DataPortalException("DataPortal.Delete " & _
        My.Resources.Failed, ex.InnerException, result.ReturnObject)
    End Try

    If portal.IsServerRemote Then
      RestoreContext(result)
    End If

    OnDataPortalInvokeComplete(New DataPortalEventArgs(dpContext))

  End Sub

#End Region

#Region " Get DataPortal Proxy "

  Private Shared mLocalPortal As DataPortalClient.IDataPortalProxy
  Private Shared mPortal As DataPortalClient.IDataPortalProxy

  Private Shared Function GetDataPortalProxy(ByVal forceLocal As Boolean) As DataPortalClient.IDataPortalProxy

    If forceLocal Then
      If mLocalPortal Is Nothing Then
        mLocalPortal = New DataPortalClient.LocalProxy
      End If
      Return mLocalPortal

    Else
      If mPortal Is Nothing Then

        Dim proxyTypeName As String = ApplicationContext.DataPortalProxy
        If proxyTypeName = "Local" Then
          mPortal = New DataPortalClient.LocalProxy

        Else
          Dim typeName As String = proxyTypeName.Substring(0, proxyTypeName.IndexOf(",")).Trim
          Dim assemblyName As String = proxyTypeName.Substring(proxyTypeName.IndexOf(",") + 1).Trim
          mPortal = DirectCast(Activator.CreateInstance(assemblyName, _
            typeName).Unwrap, DataPortalClient.IDataPortalProxy)
        End If
      End If
      Return mPortal
    End If

  End Function

#End Region

#Region " Security "

  Private Shared Function GetPrincipal() As System.Security.Principal.IPrincipal
    If ApplicationContext.AuthenticationType = "Windows" Then
      ' Windows integrated security 
      Return Nothing

    Else
      ' we assume using the CSLA framework security
      Return System.Threading.Thread.CurrentPrincipal
    End If
  End Function

#End Region

#Region " Helper methods "

  Private Shared Sub RestoreContext(ByVal result As Object)

    Dim slot As System.LocalDataStoreSlot = _
      Thread.GetNamedDataSlot("Csla.GlobalContext")
    Threading.Thread.SetData(slot, CType(result, Server.DataPortalResult).GlobalContext)

  End Sub

  Private Shared Function RunLocal(ByVal method As MethodInfo) As Boolean

    Return Attribute.IsDefined(method, GetType(RunLocalAttribute))

  End Function

  Private Shared Function GetObjectType(ByVal criteria As Object) As Type

    If criteria.GetType.IsSubclassOf(GetType(CriteriaBase)) Then
      ' get the type of the actual business object
      ' from CriteriaBase (using the new scheme)
      Return CType(criteria, CriteriaBase).ObjectType

    Else
      ' get the type of the actual business object
      ' based on the nested class scheme in the book
      Return criteria.GetType.DeclaringType
    End If

  End Function

#End Region

End Class
