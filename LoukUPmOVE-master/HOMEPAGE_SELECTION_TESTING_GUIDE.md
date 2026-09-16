# HomePage Service Selection - Testing Guide

## ?? Manual Testing Steps

### Test Environment Setup
- Platform: .NET MAUI (iOS, Android, Mac, Windows)
- App Version: Current
- Targeting: .NET 10

---

## ? Test Cases

### Test 1: Basic Selection on HomePage
**Objective**: Verify service can be selected on HomePage

**Steps**:
1. Launch app and navigate to HomePage
2. Scroll down to services section
3. Click "Ausw?hlen" button on any visible service

**Expected Results**:
- ? Toast notification appears at bottom: " „ ≈÷«›… «·Œœ„…"
- ? No errors in console
- ? Service object received in Button_Clicked_1()
- ? Console logs show: "? Service added: [ServiceName]"

**Console Output Expected**:
```
?? Service clicked: Premium Service, Price: '50.00'
? Service added: Premium Service, Price: 50
?? Current Selected Services:
   - Premium Service (Price: '50.00')
?? Total Price: 50
```

**Pass/Fail**: ___________

---

### Test 2: Multiple Selections on HomePage
**Objective**: Verify multiple services can be selected

**Steps**:
1. From Test 1 state (one service selected)
2. Click "Ausw?hlen" on a different service
3. Observe total price update

**Expected Results**:
- ? Second toast appears: " „ ≈÷«›… «·Œœ„…"
- ? Console shows both services in the list
- ? Total price = Service1 price + Service2 price
- ? Console logs show price accumulation

**Console Output Expected**:
```
?? Service clicked: Standard Service, Price: '30.00'
? Service added: Standard Service, Price: 30
?? Current Selected Services:
   - Premium Service (Price: '50.00')
   - Standard Service (Price: '30.00')
?? Total Price: 80
```

**Pass/Fail**: ___________

---

### Test 3: Deselection on HomePage
**Objective**: Verify service can be deselected (toggled off)

**Steps**:
1. From Test 2 state (two services selected)
2. Click "Ausw?hlen" on the SAME first service again
3. Observe service removal

**Expected Results**:
- ? Toast appears: " „ ≈“«·… «·Œœ„…"
- ? Console shows service removed from list
- ? Total price decreases correctly
- ? Only one service remains in SelectedServices

**Console Output Expected**:
```
?? Service clicked: Premium Service, Price: '50.00'
? Service removed: Premium Service
?? Current Selected Services:
   - Standard Service (Price: '30.00')
?? Total Price: 30
```

**Pass/Fail**: ___________

---

### Test 4: Clear All Selections
**Objective**: Verify all services can be deselected

**Steps**:
1. From Test 3 state (one service selected)
2. Click "Ausw?hlen" on remaining service
3. Verify complete deselection

**Expected Results**:
- ? Final toast: " „ ≈“«·… «·Œœ„…"
- ? SelectedServices is empty
- ? Total price = 0
- ? No services logged

**Console Output Expected**:
```
? Service removed: Standard Service
?? Current Selected Services:
?? Total Price: 0
```

**Pass/Fail**: ___________

---

### Test 5: Selection Persistence Across Navigation
**Objective**: Verify selections persist when navigating

**Steps**:
1. On HomePage, select 2 services
2. Navigate to ServicesPage (via "Alle Dienste" link or tab)
3. Observe same services are still selected
4. Navigate back to HomePage
5. Verify selections still there

**Expected Results**:
- ? Same 2 services show selected on ServicesPage
- ? Total price matches on both pages
- ? Going back to HomePage, selections remain
- ? No selection loss during navigation

**Pass/Fail**: ___________

---

### Test 6: HomePage ? ServicesPage Consistency
**Objective**: Verify behavior is identical on both pages

**Test Setup**:
1. Navigate to HomePage
2. Select 3 services: A, B, C
3. Note the total price (e.g., $100)
4. Navigate to ServicesPage
5. Verify the same 3 services are selected
6. Deselect service B on ServicesPage
7. Navigate back to HomePage

**Expected Results**:
- ? Same services shown as selected on both pages
- ? Total price updates consistently
- ? Deselection on ServicesPage reflected on HomePage
- ? No duplication of services in list
- ? No service appears twice in selections

**Pass/Fail**: ___________

---

### Test 7: Categories Filter + Selection on HomePage
**Objective**: Verify selection works when filtering by category

**Steps**:
1. On HomePage, select a category (e.g., "Hair")
2. Services list updates to show only that category
3. Select a service from filtered list
4. Switch to different category
5. Select a service from new category

**Expected Results**:
- ? Service selected from first category shows in selections
- ? Service selected from second category also added
- ? Total price includes both services
- ? No filtering errors during selection

**Pass/Fail**: ___________

---

### Test 8: Search + Selection on HomePage
**Objective**: Verify selection works with search active

**Steps**:
1. On HomePage, type in search box (e.g., "hair")
2. Services list filters by search term
3. Select a service from search results
4. Clear search
5. Verify selected service still shows in SelectedServices

**Expected Results**:
- ? Can select from search results
- ? Selection persists after clearing search
- ? Clearing search doesn't lose selection
- ? Total price maintained correctly

**Pass/Fail**: ___________

---

### Test 9: Edge Cases

#### Test 9a: No services available
**Steps**: 
1. Filter to empty category
2. Try to select (button should be disabled or hidden)

**Expected**: ? Cannot select from empty list

**Pass/Fail**: ___________

#### Test 9b: Service with invalid price
**Steps**:
1. Select a service with missing/invalid price

**Expected**: ? Graceful handling, 0 added to total

**Pass/Fail**: ___________

#### Test 9c: Rapid clicks
**Steps**:
1. Rapidly click same button multiple times
2. Observe if service is added/removed multiple times or just toggled

**Expected**: ? Service toggled once per click (add then remove then add, etc.)

**Pass/Fail**: ___________

---

### Test 10: Console Logging Verification
**Objective**: Verify all expected console logs appear

**Steps**:
1. Open app console/debug output
2. On HomePage, perform selection
3. Check console for expected messages

**Expected Console Output**:
```
?? Service clicked: [ServiceName], Price: '[Price]'
? Service added: [ServiceName], Price: [Price]
?? Current Selected Services:
   - [ServiceName] (Price: '[Price]')
?? Total Price: [Total]
```

**Verify Each Log**:
- [ ] Service clicked log
- [ ] Service added log
- [ ] Current services list
- [ ] Total price log
- [ ] No error messages
- [ ] No null reference warnings

**Pass/Fail**: ___________

---

## ?? Summary Sheet

| Test # | Test Name | Expected | Actual | Pass/Fail |
|--------|-----------|----------|--------|-----------|
| 1 | Basic Selection | Toast + Log | | |
| 2 | Multiple Selections | Price accumulates | | |
| 3 | Deselection | Service removed | | |
| 4 | Clear All | Empty list | | |
| 5 | Persistence | Selections persist | | |
| 6 | Consistency | HomePage=ServicesPage | | |
| 7 | Categories | Works with filter | | |
| 8 | Search | Works with search | | |
| 9a | No Services | Disabled/Empty | | |
| 9b | Invalid Price | Handled gracefully | | |
| 9c | Rapid Clicks | Toggled, not added multiple | | |
| 10 | Console Logs | All logs present | | |

---

## ?? Success Criteria

All tests must pass for the fix to be considered complete:
- [ ] Test 1-5: Core functionality tests
- [ ] Test 6: Consistency with ServicesPage
- [ ] Test 7-8: Feature integration tests
- [ ] Test 9: Edge cases handled
- [ ] Test 10: Debug logging working

---

## ?? Issue Report Template (If test fails)

**Test Number**: ___
**Test Name**: ___
**Expected Behavior**: ___
**Actual Behavior**: ___
**Console Output**: 
```
[Paste relevant console logs]
```
**Steps to Reproduce**:
1. ___
2. ___
3. ___
**Platform**: iOS / Android / Windows / Mac
**Severity**: Critical / High / Medium / Low

---

## ? Sign-Off

**Tester Name**: _______________  
**Date**: _______________  
**All Tests Passed**: [ ] Yes [ ] No  
**Ready for Production**: [ ] Yes [ ] No  

**Comments/Notes**:
_________________________________________________________________

---

**Testing Complete**: ___________
