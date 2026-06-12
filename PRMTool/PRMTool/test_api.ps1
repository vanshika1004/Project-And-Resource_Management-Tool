$ErrorActionPreference = "Stop"

Write-Host "1. Logging in as admin..."
$loginBody = @{ Username="admin"; Password="Admin@5678" } | ConvertTo-Json
$loginRes = Invoke-RestMethod -Uri "http://localhost:5144/api/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
Write-Host "Token received."

$token = $loginRes.token
$headers = @{ Authorization = "Bearer $token" }

$uniqueId = (New-Guid).ToString().Substring(0,4)
$newUsername = "test$uniqueId"
Write-Host "`n2. Creating new user: $newUsername"
$createUserBody = @{
    FullName = "Test User $uniqueId"
    Email = "test$uniqueId@example.com"
    Username = $newUsername
    TemporaryPassword = "TempPassword@1"
    Role = 1
} | ConvertTo-Json
$createRes = Invoke-RestMethod -Uri "http://localhost:5144/api/users" -Method Post -Body $createUserBody -ContentType "application/json" -Headers $headers
Write-Host "Created User ID: $($createRes.userId)"

Write-Host "`n3. Logging in as new user: $newUsername"
$login2Body = @{ Username=$newUsername; Password="TempPassword@1" } | ConvertTo-Json
$login2Res = Invoke-RestMethod -Uri "http://localhost:5144/api/auth/login" -Method Post -Body $login2Body -ContentType "application/json"
Write-Host "ForcePasswordChange: $($login2Res.forcePasswordChange)"

Write-Host "`n4. Changing password for new user..."
$cpBody = @{
    UserId = $createRes.userId
    CurrentPassword = "TempPassword@1"
    NewPassword = "NewPassword@123"
} | ConvertTo-Json
$cpRes = Invoke-RestMethod -Uri "http://localhost:5144/api/auth/change-password" -Method Post -Body $cpBody -ContentType "application/json"
Write-Host "Password change response: $cpRes"

Write-Host "`n5. Logging in again with new password..."
$login3Body = @{ Username=$newUsername; Password="NewPassword@123" } | ConvertTo-Json
$login3Res = Invoke-RestMethod -Uri "http://localhost:5144/api/auth/login" -Method Post -Body $login3Body -ContentType "application/json"
Write-Host "ForcePasswordChange: $($login3Res.forcePasswordChange)"
Write-Host "Success!"
