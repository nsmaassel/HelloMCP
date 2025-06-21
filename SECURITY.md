# Security Guidelines for HelloMCP

## 🔒 Security Best Practices

### ✅ **Safe to Commit**
- All configuration files in this repository use localhost URLs or environment variables
- No hardcoded API keys, tokens, or secrets
- Example configurations demonstrate proper security patterns

### ❌ **Never Commit**
- Personal access tokens (GitHub, Azure, etc.)
- API keys from external services
- Production credentials or connection strings
- User-specific MCP configurations with secrets

## 📁 **File Security Status**

### **Repository Files (Safe)**
- `.vscode/mcp.json` - ✅ Only localhost URLs and development paths
- `mcp-sdk-dotnet/appsettings*.json` - ✅ Uses environment variables
- `azure-functions-mcp/local.settings.json.example` - ✅ Example file only
- All source files - ✅ No secrets embedded

### **User Files (Not in Repository)**
- `%USERPROFILE%\.cursor\mcp.json` - ⚠️ May contain personal tokens
- Local environment variables - ⚠️ Keep private

## 🛡️ **Pre-Commit Checklist**

Before pushing changes:

1. **Run security scan:**
   ```bash
   # Search for potential secrets
   git grep -E "(ghp_|sk-|token.*[\"'][a-zA-Z0-9_-]{20,})" || echo "No secrets found"
   ```

2. **Check for new config files:**
   ```bash
   git status
   # Verify no personal config files are staged
   ```

3. **Validate environment variable usage:**
   - Ensure all secrets use `${VARIABLE_NAME}` or `Environment.GetEnvironmentVariable()`
   - No hardcoded credentials in appsettings.json

## 🔧 **Environment Variable Examples**

### Good (✅)
```json
{
  "ConnectionStrings": {
    "Database": "${DATABASE_CONNECTION_STRING}"
  },
  "ApiKeys": {
    "OpenAI": "${OPENAI_API_KEY}"
  }
}
```

### Bad (❌)
```json
{
  "ApiKeys": {
    "OpenAI": "sk-1234567890abcdef..."
  }
}
```

## 🚨 **If Secrets Are Accidentally Committed**

1. **Immediately revoke the exposed secret**
2. **Remove from git history:**
   ```bash
   git filter-branch --force --index-filter \
   'git rm --cached --ignore-unmatch path/to/file' \
   --prune-empty --tag-name-filter cat -- --all
   ```
3. **Generate new secrets**
4. **Update all systems using the old secret**

## 📞 **Security Contact**

If you discover security issues:
- Create a private issue or security advisory
- Do not expose vulnerabilities in public issues
- Follow responsible disclosure practices

---

**Remember: When in doubt, use environment variables!** 🛡️
