# TermsAndConditions CMS Refactor - Implementation & Testing Guide

## Quick Start

### What Changed?
The TermsAndConditions page now loads content **dynamically from a CMS API** instead of displaying hardcoded text. This enables non-technical staff to update terms without code changes.

### Files to Review
1. **Models**: `loukupm/Model/CmsTermsConditions.cs` - API response structure
2. **Service**: `loukupm/services/ApiServices.cs` - New `GetTermsAndConditionsAsync()` method
3. **ViewModel**: `loukupm/ViewModel/TermsAndConditionsViewModel.cs` - State management
4. **XAML**: `loukupm/View/TermsAndConditions.xaml` - Dynamic UI rendering
5. **Code-Behind**: `loukupm/View/TermsAndConditions.xaml.cs` - Lifecycle hooks

---

## How It Works (Technical Overview)

### Data Flow
```
1. User opens TermsAndConditions page
   ↓
2. OnAppearing() fires
   ↓
3. Calls LoadTermsAndConditionsCommand
   ↓
4. ViewModel sets IsLoading = true (shows spinner)
   ↓
5. ApiServices.GetTermsAndConditionsAsync() called
   ↓
6. HTTP GET to: https://test.center-yazan.com/api/pages/terms-conditions
   ↓
7. JSON response deserialized into TermsConditionsData
   ↓
8. ViewModel updates CmsData property (bound to XAML)
   ↓
9. XAML renders blocks dynamically using BindableLayout
   ↓
10. Each block type rendered according to its template
```

### State Management
```csharp
// Page starts in initial state
IsLoading = false, HasError = false, CmsData = null

// While loading
IsLoading = true  → LoadingIndicator visible, content hidden

// On success
IsLoading = false, CmsData = response.Data → content displays

// On failure
HasError = true, ErrorMessage = "Error description" → error UI shows
User can click Retry to retry the load
```

---

## Supported CMS Block Types

### 1. Heading (`type: "heading"`)
```json
{
  "type": "heading",
  "id": "h1-intro",
  "props": { "level": 1 },
  "content": { "text": "Welcome to Terms & Conditions" }
}
```
**Renders as**: Large gold bold text in frame

### 2. Paragraph (`type: "paragraph"`)
```json
{
  "type": "paragraph",
  "id": "p1",
  "props": {},
  "content": { "text": "By using this service, you agree to..." }
}
```
**Renders as**: Regular white text that wraps to multiple lines

### 3. Divider (`type: "divider"`)
```json
{
  "type": "divider",
  "id": "div1",
  "props": {},
  "content": {}
}
```
**Renders as**: Thin horizontal line

### 4. Unordered List (`type: "unordered_list"`)
```json
{
  "type": "unordered_list",
  "id": "list1",
  "props": {},
  "content": {
	"items": [
	  "You must be 18 years old",
	  "You agree not to abuse the service",
	  "You understand our liability limitations"
	]
  }
}
```
**Renders as**: Bulleted list with gold bullets

### 5. Warning Box (`type: "warning_box"`)
```json
{
  "type": "warning_box",
  "id": "warning1",
  "props": {},
  "content": { "text": "Important: Termination of account may result in data loss" }
}
```
**Renders as**: Gold-bordered box with warning icon

---

## Testing the Implementation

### Prerequisites
- Latest build deployed to device/emulator
- Network connectivity available
- CMS API endpoint accessible

### Manual Test Cases

#### Test 1: Happy Path (Successful Load)
**Steps**:
1. Open TermsAndConditions page
2. Observe loading indicator appears
3. Wait for API response (~2 seconds)
4. Verify all content loads and displays correctly
5. Verify all block types render properly
6. Try scrolling through content

**Expected Result**: ✅ Content loads, all blocks visible, smooth scrolling

---

#### Test 2: Error Handling
**Steps**:
1. Disconnect device from network
2. Open TermsAndConditions page
3. Observe loading indicator
4. Wait for timeout (~30 seconds)
5. Verify error message appears
6. Verify "Retry" button appears
7. Click Retry button
8. Verify error persists (offline)

**Expected Result**: ✅ Error message shown, retry button works

---

#### Test 3: RTL Support (Arabic)
**Steps**:
1. Change app language to Arabic (Settings)
2. Open TermsAndConditions page
3. Verify content loads
4. Verify text displays right-to-left
5. Verify layout is mirrored appropriately

**Expected Result**: ✅ Content displays RTL, no layout breaks

---

#### Test 4: Long Content
**Steps**:
1. Open TermsAndConditions page
2. Verify content loads
3. Scroll to bottom of page
4. Verify all blocks display
5. Verify footer "Last Updated" displays at bottom
6. Scroll back to top

**Expected Result**: ✅ All content accessible, smooth scrolling

---

#### Test 5: Empty Content
**Steps** (if API returns empty blocks):
1. Configure API to return `"blocks": []`
2. Open TermsAndConditions page
3. Verify loading indicator appears
4. Verify "No Content Available" message shows

**Expected Result**: ✅ Empty state handled gracefully

---

#### Test 6: Retry After Connection Recovery
**Steps**:
1. Disconnect network, trigger error
2. Reconnect network
3. Click Retry button
4. Verify content loads successfully

**Expected Result**: ✅ Content loads after retry

---

### Console Logging (Debugging)

**Watch for these log messages**:

```
✅ Terms & Conditions data retrieved successfully
✅ Terms & Conditions loaded successfully
   Language: en
   Direction: ltr
   Blocks: 8

❌ Terms & Conditions API error: [StatusCode]
❌ Exception while loading Terms and Conditions data: [Error]
🔄 Retrying Terms and Conditions load...
```

---

## Common Issues & Solutions

### Issue 1: "No Content Available" on first load
**Diagnosis**:
- Check network connectivity
- Verify API endpoint is accessible
- Check SSL certificate is valid

**Solution**:
1. Restart app
2. Tap Retry button
3. Check network connection
4. Verify API endpoint in code

---

### Issue 2: Loading spinner never stops (hangs)
**Diagnosis**:
- API endpoint not responding
- Network timeout
- API error without exception

**Solution**:
1. Check network logs
2. Increase timeout if needed: `_httpClient.Timeout = TimeSpan.FromSeconds(60)`
3. Check if API server is running
4. Verify endpoint URL in ApiServices

---

### Issue 3: Text displays incorrectly for RTL languages
**Diagnosis**:
- Direction not set in API response
- FlowDirection not updating

**Solution**:
1. Verify API returns `"direction": "rtl"` for Arabic
2. Restart app to apply language change
3. Check ViewModel's ContentFlowDirection property updates

---

### Issue 4: List items don't appear
**Diagnosis**:
- Block type is not "unordered_list"
- Items collection is empty
- Items are not strings

**Solution**:
1. Check CMS block type is exactly "unordered_list"
2. Verify items array contains string values
3. Check no null or empty items

---

## Performance Optimization Tips

### For CMS Administrators
- **Keep paragraphs concise** - Optimal reading on mobile (~50-100 words per paragraph)
- **Use headings to structure** - Break up content with H1/H2 headings
- **Limit list items** - Avoid lists with 20+ items (create multiple lists instead)
- **Test on mobile** - Preview rendering on actual device before publishing

### For Developers
- **Enable caching** - Cache response for 1 hour to reduce API calls
- **Add retry policy** - Implement exponential backoff for failed requests
- **Profile rendering** - Monitor frame rate when rendering 100+ blocks
- **Optimize list rendering** - Consider pagination for very large lists

---

## Adding More Block Types

### How to Add a New Block Type

**Example: Adding an "image_block" type**

#### Step 1: Update Model (CmsTermsConditions.cs)
No change needed if image URL is in `content.text`

#### Step 2: Update XAML (TermsAndConditions.xaml)
Add inside the BindableLayout.ItemTemplate:

```xaml
<!-- Image Block -->
<Image IsVisible="{Binding Type, StringFormat='image_block'}"
	   Source="{Binding Content.Text}"
	   Aspect="AspectFit"
	   HeightRequest="300"
	   Margin="0,10,0,10" />
```

#### Step 3: Update CMS API
Include block in response:
```json
{
  "type": "image_block",
  "id": "img1",
  "props": {},
  "content": { "text": "https://example.com/image.png" }
}
```

#### Step 4: Test
- Verify image renders correctly
- Test with various image sizes
- Verify RTL layout still works

---

## API Integration Checklist

Before connecting to production CMS:

### Endpoint Configuration
- [ ] Endpoint URL is correct
- [ ] SSL certificate is valid
- [ ] API accepts GET requests
- [ ] CORS headers allow app domain (if needed)

### Response Format
- [ ] Response includes `success` boolean
- [ ] Response includes `data` object
- [ ] Data includes `blocks` array
- [ ] Each block has `type`, `id`, `props`, `content`
- [ ] Content has `text` for text types, `items` for lists

### Error Handling
- [ ] API returns proper HTTP status codes
- [ ] Error responses include `message` field
- [ ] Timeout is set appropriately (30+ seconds)
- [ ] Network errors are handled gracefully

### Performance
- [ ] Response time < 2 seconds for typical content
- [ ] Response payload < 500KB
- [ ] No excessive API calls (caching recommended)
- [ ] Works on 3G networks

---

## Developer Reference

### Key Properties (ViewModel)

| Property | Type | Binding | Purpose |
|----------|------|---------|---------|
| `CmsData` | `TermsConditionsData` | `{Binding CmsData}` | Main content |
| `IsLoading` | `bool` | `{Binding IsLoading}` | Show loading UI |
| `HasError` | `bool` | `{Binding HasError}` | Show error UI |
| `ErrorMessage` | `string` | `{Binding ErrorMessage}` | Display error text |
| `PageDirection` | `string` | `{Binding PageDirection}` | "rtl" or "ltr" |
| `ContentFlowDirection` | `FlowDirection` | `{Binding ContentFlowDirection}` | XAML flow direction |

### Key Commands (ViewModel)

| Command | Parameters | Purpose |
|---------|-----------|---------|
| `LoadTermsAndConditionsCommand` | None | Initial load |
| `RetryLoadTermsAndConditionsCommand` | None | Retry after error |

### Key Methods (ViewModel)

| Method | Purpose |
|--------|---------|
| `ClearData()` | Reset all state |

---

## Maintenance Guide

### Regular Checks
- [ ] Monitor API uptime and performance
- [ ] Review error logs weekly
- [ ] Update content via CMS when needed
- [ ] Test new block types before publishing

### Updates
- When updating block types:
  1. First update XAML template
  2. Test thoroughly
  3. Deploy to staging
  4. Verify with CMS API
  5. Deploy to production

- When changing API response structure:
  1. Update models first
  2. Update service method
  3. Update ViewModel
  4. Test end-to-end
  5. Deploy with blue-green strategy

---

## Support & Questions

### Debugging Tips
1. **Check console logs** - Run with Debug configuration for detailed output
2. **Enable network debugging** - Monitor HTTP requests with Charles or Fiddler
3. **Test API directly** - Use Postman to verify API response format
4. **Verify data binding** - Check XAML binding paths match property names

### Common Questions

**Q: How do I update the terms?**
A: Update via the CMS API admin panel. No code deployment needed.

**Q: How often does it refresh?**
A: Every time the page opens. Implement caching if needed.

**Q: Can I have RTL and LTR content together?**
A: Yes! API specifies direction per page, so each language version can set its own.

**Q: What happens if the API is down?**
A: User sees error message with Retry button. Content doesn't load.

**Q: Can I customize the styling?**
A: Yes! Edit colors in TermsAndConditions.xaml. Colors can also come from CMS.

---

## Next Steps

1. **Deploy to Staging** - Test with actual CMS endpoint
2. **QA Testing** - Run full test suite
3. **Performance Testing** - Test with various content sizes
4. **User Acceptance Testing** - Get feedback from stakeholders
5. **Deploy to Production** - Release to users
6. **Monitor** - Watch for errors in production logs

---

**Last Updated**: 2024  
**Version**: 1.0  
**Status**: Ready for Production Deployment ✅
