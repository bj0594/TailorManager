# Specification – TailorManager

## 1 The project in my own words

TailorManager is an order management system for tailoring businesses that handle new garment orders and repairs.
Employees can register customers and orders, track workshop progress, schedule fittings, communicate with customers, and register payments upon collection.
The system will have a .NET-based backend and an employee-facing interface, with frontend technology and deployment still undecided.

Parts: FrontDesk, Workshop, Core


## 2 Requirements

Parts of the system:
- FrontDesk (register incoming orders, handle payment and deliver completed orders)
- Workshop (process orders, schedule fittings and communicate with customers)

Main flow (FrontDesk): An employee registers a customer and an order, submits it to the Workshop, and later registers payment and delivers the order when the customer collects it.

Main flow (Workshop): An employee starts a submitted order, performs the work, pauses when waiting for customer decisions or fittings, communicates with the customer, and marks the order as ready for pickup when completed.

Core (MUST):

- (FrontDesk) Register a customer and create an order for a new garment or a repair, including its work description and initially agreed price. An order can optionally be linked to a previous order.
- (FrontDesk → Workshop) Submit an order so that the Workshop can access its customer information and work details.
- (Workshop) Start an order, record work performed, and update its status and history.
- (Workshop) Pause an order with the status WaitingForCustomer when a customer decision is required, record the written response, and resume work when the decision is resolved.
- (Workshop) Schedule fittings, pause an order with the status WaitingForFitting, and record fitting results before resuming work.
- (Workshop) Record incoming and outgoing customer communication and maintain a chronological history of all relevant order activities.
- (Workshop → FrontDesk) Mark a completed order as ReadyForPickup and make it available to FrontDesk.
- (FrontDesk) Register confirmed full payment and automatically change the order status to Delivered.

Rules:

- An order follows Submitted → InProgress → ReadyForPickup → Delivered, with WaitingForCustomer and WaitingForFitting as temporary states that return to InProgress.
- Work cannot be completed or an order marked ReadyForPickup while waiting for a customer decision or fitting.
- An order in WaitingForFitting must have a scheduled fitting date.
- An order has an initially agreed price when created. Changes to agreed work or price require written customer approval recorded in the order history.
- A proposed price adjustment does not affect the agreed price until the customer has approved it in writing.
- An order cannot be marked Delivered before full payment has been registered.
- Failed communication does not change the order status, but its failure is recorded in the history.
- Delivered orders cannot be reopened. Reclamation work is registered as a new order linked to the original order.
- Order history is append-only: previous activities are not overwritten or deleted.

Later / bonus (NOT NOW):

- Partial payments and deposits.
- Order cancellation and cancellation fees.
- Customer-facing order tracking.
- Inventory and material management.
- Accounting integration.
- Direct email and SMS provider integrations.
- Payment terminal integration.
- Advanced reporting and business analytics.



