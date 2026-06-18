import random
from datetime import datetime, timedelta
import string
import uuid

def generate_sql():
    sql = []
    sql.append("-- ===========================================")
    sql.append("-- DUMMY DATA SCRIPT FOR HR MANAGEMENT PLATFORM")
    sql.append("-- ===========================================\n")
    
    # 1. Departments
    sql.append("PRINT 'Inserting Departments...'")
    sql.append("SET IDENTITY_INSERT kumarcapstone_Departments ON;")
    
    departments = [
        "Human Resources", "Information Technology", "Finance", 
        "Marketing", "Sales", "Operations", 
        "Administration", "Customer Support", "Research & Development", 
        "Quality Assurance"
    ]
    
    for i, name in enumerate(departments):
        # We start IDs from 1. If 1 exists, it's fine, we'll assume we can insert or DB is clean.
        # Actually to avoid conflicts, let's start IDs from 101.
        dept_id = 100 + i + 1
        sql.append(f"INSERT INTO kumarcapstone_Departments (Id, Name, CreatedDate, IsDeleted) VALUES ({dept_id}, '{name}', GETUTCDATE(), 0);")
    
    sql.append("SET IDENTITY_INSERT kumarcapstone_Departments OFF;\n")
    
    # 2. Employees
    sql.append("PRINT 'Inserting Employees...'")
    sql.append("SET IDENTITY_INSERT kumarcapstone_Employees ON;")
    
    first_names = ["John", "Emma", "David", "Michael", "Sarah", "James", "William", "Olivia", "Sophia", "Daniel"]
    last_names = ["Smith", "Wilson", "Jones", "Brown", "Taylor", "Miller", "Anderson", "Thomas", "Jackson", "White"]
    
    employees = []
    
    for i in range(100):
        emp_id = 1000 + i + 1
        dept_id = 101 + (i % 10)
        
        fname = random.choice(first_names)
        lname = random.choice(last_names)
        # Ensure uniqueness by appending ID
        email = f"{fname.lower()}.{lname.lower()}{emp_id}@hrmanagement.com"
        
        emp_code = f"EMP-2026-{str(i+1).zfill(4)}"
        phone = f"555-{str(random.randint(100,999))}-{str(random.randint(1000,9999))}"
        gender = random.choice(["Male", "Female"])
        designation = "Employee"
        if i < 5:
            designation = "Admin"
        elif i < 15:
            designation = "HR Manager"
            
        join_date = f"202{(i%5)}-0{1 + (i%9)}-{10 + (i%15)}T00:00:00.0000000"
        
        sql.append(f"INSERT INTO kumarcapstone_Employees (Id, EmployeeCode, FirstName, LastName, Email, PhoneNumber, Gender, Designation, DateOfJoining, EmploymentStatus, DepartmentId, CreatedDate, IsDeleted) "
                   f"VALUES ({emp_id}, '{emp_code}', '{fname}', '{lname}', '{email}', '{phone}', '{gender}', '{designation}', '{join_date}', 'Active', {dept_id}, GETUTCDATE(), 0);")
        employees.append({
            "id": emp_id,
            "email": email,
            "role": designation
        })
        
    sql.append("SET IDENTITY_INSERT kumarcapstone_Employees OFF;\n")
    
    # 3. AspNetUsers (Identity)
    sql.append("PRINT 'Inserting AspNetUsers...'")
    sql.append("DECLARE @adminHash NVARCHAR(MAX) = (SELECT TOP 1 PasswordHash FROM kumarcapstone_AspNetUsers WHERE Email = 'admin@hrmanagement.com');")
    sql.append("IF @adminHash IS NULL SET @adminHash = 'AQAAAAIAAYagAAAAEGv...'; -- fallback if empty")
    
    for emp in employees:
        user_id = str(uuid.uuid4())
        email = emp["email"]
        emp_id = emp["id"]
        
        sql.append(f"INSERT INTO kumarcapstone_AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount, IsActive, CreatedDate, EmployeeId, ThemePreference, EmailAlertsEnabled, InAppNotificationsEnabled) "
                   f"VALUES ('{user_id}', '{email}', '{email.upper()}', '{email}', '{email.upper()}', 1, @adminHash, NEWID(), NEWID(), 0, 0, 1, 0, 1, GETUTCDATE(), {emp_id}, 'light', 1, 1);")
        
        # Link role (assuming roles exist or we can just use role names if we don't know Role Id exactly... wait, we need RoleId)
        # We can do this dynamically:
        role_name = emp["role"]
        sql.append(f"INSERT INTO kumarcapstone_AspNetUserRoles (UserId, RoleId) SELECT '{user_id}', Id FROM kumarcapstone_AspNetRoles WHERE Name = '{role_name}';")

    sql.append("\n")
    
    # 4. Leave Types & Balances
    sql.append("PRINT 'Inserting LeaveTypes and Balances...'")
    # Assuming Leave Types might not exist, but let's just create 3 basic ones or use existing ones. We can check if they exist first.
    # Actually, the user's instructions didn't explicitly ask to generate LeaveTypes, but asked for Leave Requests.
    # We will assume Leave Types exist, or we will just create dummy ones with IDs 101, 102.
    sql.append("IF NOT EXISTS (SELECT 1 FROM kumarcapstone_LeaveTypes) BEGIN")
    sql.append("  SET IDENTITY_INSERT kumarcapstone_LeaveTypes ON;")
    sql.append("  INSERT INTO kumarcapstone_LeaveTypes (Id, Name, DefaultAllocationDays, CreatedDate, IsDeleted) VALUES (101, 'Casual Leave', 12, GETUTCDATE(), 0);")
    sql.append("  INSERT INTO kumarcapstone_LeaveTypes (Id, Name, DefaultAllocationDays, CreatedDate, IsDeleted) VALUES (102, 'Sick Leave', 12, GETUTCDATE(), 0);")
    sql.append("  SET IDENTITY_INSERT kumarcapstone_LeaveTypes OFF;")
    sql.append("END\n")

    # Leave balances
    sql.append("DECLARE @leaveType1 INT = (SELECT TOP 1 Id FROM kumarcapstone_LeaveTypes WHERE Name = 'Casual Leave');")
    sql.append("IF @leaveType1 IS NULL SET @leaveType1 = 101;")
    for emp in employees:
        emp_id = emp["id"]
        sql.append(f"INSERT INTO kumarcapstone_LeaveBalances (EmployeeId, LeaveTypeId, AllocatedDays, UsedDays, PendingDays, Year, CreatedDate, IsDeleted) VALUES ({emp_id}, @leaveType1, 12, 0, 12, 2026, GETUTCDATE(), 0);")
        
    sql.append("\n")

    # 5. Leave Requests
    sql.append("PRINT 'Inserting Leave Requests...'")
    sql.append("SET IDENTITY_INSERT kumarcapstone_LeaveRequests ON;")
    statuses = ["Pending", "Approved", "Rejected"]
    for i in range(200):
        req_id = 1000 + i + 1
        emp = random.choice(employees)
        emp_id = emp["id"]
        status = random.choice(statuses)
        
        sql.append(f"INSERT INTO kumarcapstone_LeaveRequests (Id, EmployeeId, LeaveTypeId, StartDate, EndDate, TotalDays, Reason, Status, CreatedDate, IsDeleted) "
                   f"VALUES ({req_id}, {emp_id}, @leaveType1, '2026-07-01T00:00:00', '2026-07-02T00:00:00', 2, 'Personal reasons', '{status}', GETUTCDATE(), 0);")
    sql.append("SET IDENTITY_INSERT kumarcapstone_LeaveRequests OFF;\n")
    
    # 6. Notifications
    sql.append("PRINT 'Inserting Notifications...'")
    sql.append("SET IDENTITY_INSERT kumarcapstone_Notifications ON;")
    messages = ["Employee Added", "Employee Updated", "Department Created", "Leave Request Submitted", "Leave Approved"]
    for i in range(300):
        notif_id = 1000 + i + 1
        emp = random.choice(employees)
        emp_id = emp["id"]
        msg = random.choice(messages)
        is_read = random.choice([0, 1])
        
        sql.append(f"INSERT INTO kumarcapstone_Notifications (Id, EmployeeId, Message, IsRead, Type, CreatedDate, IsDeleted) "
                   f"VALUES ({notif_id}, {emp_id}, '{msg}', {is_read}, 'System', GETUTCDATE(), 0);")
    sql.append("SET IDENTITY_INSERT kumarcapstone_Notifications OFF;\n")
    
    # 7. Audit Logs
    sql.append("PRINT 'Inserting Audit Logs...'")
    sql.append("SET IDENTITY_INSERT kumarcapstone_AuditLogs ON;")
    actions = ["Create", "Update", "Delete", "Restore", "Login", "Logout"]
    tables = ["kumarcapstone_Employees", "kumarcapstone_Departments", "kumarcapstone_LeaveRequests"]
    for i in range(500):
        audit_id = 1000 + i + 1
        action = random.choice(actions)
        table = random.choice(tables)
        
        sql.append(f"INSERT INTO kumarcapstone_AuditLogs (Id, Action, TableName, PrimaryKey, Timestamp, CreatedDate, IsDeleted) "
                   f"VALUES ({audit_id}, '{action}', '{table}', '1', GETUTCDATE(), GETUTCDATE(), 0);")
    sql.append("SET IDENTITY_INSERT kumarcapstone_AuditLogs OFF;\n")
    
    sql.append("PRINT 'Dummy data generation completed successfully!'")
    
    with open("dummy_data_seed.sql", "w", encoding="utf-8") as f:
        f.write("\n".join(sql))
        
if __name__ == '__main__':
    generate_sql()
