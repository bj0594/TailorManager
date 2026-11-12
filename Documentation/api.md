# API

Enums are sent as text, like `"NewGarment"` and `"Submitted"`. Times are ISO 8601 with offset, like `"2026-01-01T09:00:00+00:00"`.

Every endpoint except register and login needs a token from POST /auth/login. The role in parentheses after Auth is the part that may use the endpoint. The history records the logged-in employee from the token.

```
GET /health
Request: none
Response: 200 OK + Healthy
Errors: 503 Service Unavailable + Unhealthy (a dependency does not answer)

POST /auth/register
Request: { "username": "kari", "password": "Secret-123", "firstName": "Kari", "lastName": "Nordmann", "role": "FrontDesk" }
Response: 201 Created + { "id": "2c1f4a7e-9d3b-4e8a-b6f5-1a2b3c4d5e6f", "username": "kari", "displayName": "Kari N.", "role": "FrontDesk" }
Errors: 400 Bad Request (empty first name or last name)
        400 Bad Request (role is not FrontDesk or Workshop)
        409 Conflict (username is taken)

POST /auth/login
Request: { "username": "kari", "password": "Secret-123" }
Response: 200 OK + { "token": "eyJ…" }
Errors: 401 Unauthorized (wrong username or password)

POST /customers
Auth: Bearer token (FrontDesk)
Request: { "name": "Anna Hansen", "phone": "98765432", "email": "anna@example.com", "address": "Storgata 1, 0155 Oslo" }
Response: 201 Created + { "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "name": "Anna Hansen", "phone": "98765432", "email": "anna@example.com", "address": "Storgata 1, 0155 Oslo" }
Errors: 401 Unauthorized (no valid token)
        403 Forbidden (not FrontDesk)
        400 Bad Request (empty name, phone, email or address)

GET /customers?searchTerm={searchTerm}
Auth: Bearer token (FrontDesk)
Request: none
Response: 200 OK + array of the customers whose name, phone or email matches, each with the same fields as in the POST /customers response
Errors: 401 Unauthorized (no valid token)
        403 Forbidden (not FrontDesk)

GET /customers/{customerId}
Auth: Bearer token (FrontDesk, Workshop)
Request: none
Response: 200 OK + the customer, with the same fields as in the POST /customers response
Errors: 401 Unauthorized (no valid token)
        404 Not Found (customer does not exist)

POST /orders
Auth: Bearer token (FrontDesk)
Request: { "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "type": "NewGarment", "workDescription": "Shorten the trousers by 3 cm", "agreedPrice": 450, "relatedOrderId": null }
Response: 201 Created + {
  "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "type": "NewGarment",
  "workDescription": "Shorten the trousers by 3 cm",
  "initialAgreedPrice": 450,
  "currentAgreedPrice": 450,
  "outstandingBalance": 450,
  "status": "Submitted",
  "createdAt": "2026-01-01T09:00:00+00:00",
  "relatedOrderId": null,
  "approvedPriceAdjustments": [],
  "history": [ { "id": "1b4e28ba-2fa1-11d2-883f-0016d3cca427", "type": "OrderCreated", "occurredAt": "2026-01-01T09:00:00+00:00", "actorId": "2c1f4a7e-9d3b-4e8a-b6f5-1a2b3c4d5e6f", "actorDisplayName": "Kari N.", "relatedEntityId": null, "note": null, "fromStatus": null, "toStatus": "Submitted" } ],
  "fittings": [],
  "customerDecisionRequests": [],
  "customerApprovals": [],
  "communications": [],
  "payments": []
}
Errors: 401 Unauthorized (no valid token)
        403 Forbidden (not FrontDesk)
        400 Bad Request (negative price or empty work description)
        404 Not Found (customer does not exist)
        404 Not Found (related order does not exist)
        409 Conflict (related order belongs to another customer)

POST /orders/{orderId}/approvals
Auth: Bearer token (FrontDesk)
Request: { "method": "SignedForm", "documentReference": "FORM-1001" }
Response: 201 Created + { "id": "9b2d5f3a-6c1e-4f8a-b7d2-3e4f5a6b7c8d", "scope": "InitialOrder", "method": "SignedForm", "approvedAt": "2026-01-01T09:00:00+00:00", "relatedDecisionRequestId": null, "communicationId": null, "documentReference": "FORM-1001", "approvedAmount": 450 }
Errors: 401 Unauthorized (no valid token)
        403 Forbidden (not FrontDesk)
        400 Bad Request (signed form without a document reference)
        404 Not Found (order does not exist)

POST /orders/{orderId}/inspection-fee-agreements
Auth: Bearer token (FrontDesk)
Request: { "amount": 150, "documentReference": "FORM-1002" }
Response: 201 Created + the approval, with the same fields as in the POST /orders/{orderId}/approvals response, and "scope": "InspectionFee"
Errors: 401 Unauthorized (no valid token)
        403 Forbidden (not FrontDesk)
        400 Bad Request (negative amount or missing document reference)
        404 Not Found (order does not exist)

GET /orders?status={status}&customerId={customerId}
Auth: Bearer token (FrontDesk, Workshop)
Request: none
Response: 200 OK + array of the orders that match, all orders when both are left out, each with the same fields as in the POST /orders response
Errors: 401 Unauthorized (no valid token)

GET /orders/{orderId}
Auth: Bearer token (FrontDesk, Workshop)
Request: none
Response: 200 OK + the order with its history, with the same fields as in the POST /orders response
Errors: 401 Unauthorized (no valid token)
        404 Not Found (order does not exist)

PATCH /orders/{orderId}/start
Auth: Bearer token (Workshop)
Request: none
Response: 200 OK + the order, with "status": "InProgress" and a StatusChanged entry in "history"
Errors: 401 Unauthorized (no valid token)
        403 Forbidden (not Workshop)
        404 Not Found (order does not exist)
        409 Conflict (order is not Submitted)

POST /orders/{orderId}/notes
Auth: Bearer token (Workshop)
Request: { "note": "Hem pinned, ready to sew" }
Response: 200 OK + the order, with a WorkNoteAdded entry in "history" that has the note
Errors: 401 Unauthorized (no valid token)
        403 Forbidden (not Workshop)
        400 Bad Request (empty note)
        404 Not Found (order does not exist)
        409 Conflict (order is not InProgress)

POST /orders/{orderId}/decision-requests
Auth: Bearer token (Workshop)
Request: { "proposedWorkChange": "Also take in the waist by 2 cm", "proposedPriceAdjustment": 150 }
Response: 200 OK + the order, with a pending request in "customerDecisionRequests" and "status": "WaitingForCustomer"
Also: raises a notification request to the customer when the status changes
Errors: 401 Unauthorized (no valid token)
        403 Forbidden (not Workshop)
        400 Bad Request (empty work change)
        404 Not Found (order does not exist)
        409 Conflict (order is not InProgress or WaitingForCustomer)

POST /orders/{orderId}/decision-requests/{requestId}/decision
Auth: Bearer token (Workshop)
Request: { "communicationId": "5f8d2c1e-3a4b-4c6d-8e9f-0a1b2c3d4e5f", "approved": true }
Response: 200 OK + the order, with the decision recorded, a new approval and price adjustment when approved, and "status": "InProgress" when no request is pending
Errors: 401 Unauthorized (no valid token)
        403 Forbidden (not Workshop)
        404 Not Found (order does not exist)
        404 Not Found (decision request does not exist)
        409 Conflict (no recorded written response)

POST /orders/{orderId}/fittings
Auth: Bearer token (Workshop)
Request: { "scheduledAt": "2026-01-03T10:00:00+00:00" }
Response: 200 OK + the order, with the appointment in "fittings" and "status": "WaitingForFitting"
Errors: 401 Unauthorized (no valid token)
        403 Forbidden (not Workshop)
        400 Bad Request (the date is before the current time)
        404 Not Found (order does not exist)
        409 Conflict (order is not InProgress or WaitingForFitting)

PATCH /orders/{orderId}/fittings/{fittingId}/complete
Auth: Bearer token (Workshop)
Request: { "outcome": "The length is right" }
Response: 200 OK + the order, with the outcome on the fitting, and "status": "InProgress" when every fitting is completed
Errors: 401 Unauthorized (no valid token)
        403 Forbidden (not Workshop)
        404 Not Found (order does not exist)
        404 Not Found (fitting does not exist)
        409 Conflict (order is not WaitingForFitting)

POST /orders/{orderId}/communications
Auth: Bearer token (Workshop)
Request: { "channel": "Email", "direction": "Incoming", "source": "Customer", "content": "Yes, please take in the waist as well.", "outcome": "Received", "occurredAt": "2026-01-02T14:00:00+00:00" }
Response: 200 OK + the order, with the entry in "communications" and a CommunicationRecorded entry in "history"
Errors: 401 Unauthorized (no valid token)
        403 Forbidden (not Workshop)
        400 Bad Request (empty content)
        404 Not Found (order does not exist)

PATCH /orders/{orderId}/ready-for-pickup
Auth: Bearer token (Workshop)
Request: none
Response: 200 OK + the order, with "status": "ReadyForPickup"
Also: raises a notification request to the customer when the status changes
Errors: 401 Unauthorized (no valid token)
        403 Forbidden (not Workshop)
        404 Not Found (order does not exist)
        409 Conflict (order is not InProgress)

POST /orders/{orderId}/payments
Auth: Bearer token (FrontDesk)
Request: { "amount": 450, "transactionReference": "TX-1001" }
Response: 200 OK + the order, with the payment in "payments", "outstandingBalance": 0 and "status": "Delivered"
Errors: 401 Unauthorized (no valid token)
        403 Forbidden (not FrontDesk)
        404 Not Found (order does not exist)
        409 Conflict (order is not ReadyForPickup)
        409 Conflict (amount is not the outstanding balance)
        409 Conflict (transaction is already registered)

PATCH /orders/{orderId}/delivered
Auth: Bearer token (FrontDesk)
Request: none
Response: 200 OK + the order, with "status": "Delivered"
Errors: 401 Unauthorized (no valid token)
        403 Forbidden (not FrontDesk)
        404 Not Found (order does not exist)
        409 Conflict (order is not ReadyForPickup)
        409 Conflict (outstanding balance is not zero)
```
