const priceButtons = document.getElementById('pricebutons');
const numberContainer = document.getElementById('numbercontainer');
const buttonsInsideContainer = numberContainer.querySelectorAll('button');
const allButtons = document.querySelectorAll('button');
var betelement = document.getElementById("TotalBet");
let isConfirm = false;
let batingInfoArray = [];
let totalBetAmmount = 0;
let bettingJson = "";
let checkBalance = true;

function handleButtonClick(event) {

    if (!isConfirm) {

        const clickedButton = event.target;
        const dataValue = clickedButton.getAttribute('data-value');
        const selectedNumber = parseInt(dataValue);
        const selectedAmountInput = priceButtons.querySelector('input[type="radio"]:checked');
        const selectedAmount = selectedAmountInput.checked ? parseInt(selectedAmountInput.value) : 0;
        const storedWalletBalance = localStorage.getItem('walletBalance');

        if (storedWalletBalance !== null) {


            const walletBalance = parseInt(storedWalletBalance);
            totalBetAmmount += selectedAmount;
            if (walletBalance >= totalBetAmmount) {


                betelement.innerText = totalBetAmmount;

                clickedButton.classList.remove('btn-outline-dark');
                clickedButton.classList.add('btn-dark');

                const badge = clickedButton.querySelector('.badge-top');

                const bettingInfo = batingInfoArray.find(info => info.number === selectedNumber);

                if (bettingInfo) {
                    bettingInfo.amount += selectedAmount;
                    badge.innerText = `${bettingInfo.amount}₹`;
                }
                else {
                    addBadgeToButton(clickedButton, selectedAmount);

                    const bettingInfo = {
                        number: selectedNumber,
                        amount: selectedAmount
                    };
                    batingInfoArray.push(bettingInfo);
                }
            } else {

                totalBetAmmount -= selectedAmount;
                checkBalance = false;
                betelement.innerText = totalBetAmmount;
                ShowAlert("failure", "Your bet reached to your wallet balance please recharge your wallet to continoue!!");

            }

        }
    }


    console.log('Betting Info Array:', batingInfoArray);
}

function ShowAlert(type, message) {
    const modal = document.getElementById('notificationModal');
    const icon = document.getElementById('notificationIcon');
    const messageElement = document.getElementById('notificationMessage');

    if (type === 'success') {
        icon.className = 'fas fa-check-circle text-success';
    } else if (type === 'failure') {
        icon.className = 'fas fa-times-circle text-danger';
    }

    messageElement.textContent = message;
    $(modal).modal('show'); // Use jQuery to show the modal
}



function addBadgeToButton(targetButton, ammount) {

    const badgeContainer = document.createElement('span');
    badgeContainer.classList.add('badge-top');
    badgeContainer.innerHTML = `${ammount}₹`;
    targetButton.appendChild(badgeContainer);
}

function clearBettingButtonBadge() {

    const darkButtons = document.querySelectorAll('.btn-dark');
    totalBetAmmount = 0;
    betelement.innerText = totalBetAmmount;
    darkButtons?.forEach((button) => {
        button.classList.remove('btn-dark');
        button.classList.add('btn-outline-dark');
        const badgeContainer = button.querySelector('.badge-top');
        badgeContainer.remove();

    });
    batingInfoArray = [];

}

//Function to disable all buttons
function disableButtons() {
    allButtons.forEach((button) => {
        button.disabled = true;
    });
    checkboxes.forEach((check) => { check.disabled = true })
}

// Function to enable all buttons
function enableButtons() {
    allButtons.forEach((button) => {
        button.disabled = false;
    });
    checkboxes.forEach((x) => x.disabled = false);
}

// Event listinger for all number button
buttonsInsideContainer.forEach((button) => {
    button.addEventListener('click', handleButtonClick);
});


// Connect to the SignalR hub
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/updateHub")
    .build();

// Define a method to handle updates received from the server
connection.on("updateOpenNumberAndList", (message) => {

    clearBettingButtonBadge();
    isConfirm = false;
    const previousOpenings = document.getElementById("previousOpenings");
    previousOpenings.innerHTML = "";
    message.forEach((x,index) => {
        const newElement = document.createElement("div");
        if (index == 0) {
            newElement.className = "border border-danger bg-danger rounded m-1 p-1";
            newElement.innerText = (x < 10 ? `0${x}` : x);
        } else {
            newElement.className = "border border-success bg-success rounded m-1 p-1";
            newElement.innerText = (x < 10 ? `0${x}` : x);
        }
        
        previousOpenings.appendChild(newElement);
    });
    const openNumerElement = document.getElementById("OpenNumber");
    openNumerElement.innerText = (message[0] < 10 ? `0${message[0]}` : message[0]);

});
connection.on("NotifyWinner", (dataJson) => {

    let numberFeild = document.getElementById("WinningNumber");
    let priceFeild = document.getElementById("WinningPrice");

    numberFeild.innerText = dataJson.winningNumber;
    priceFeild.innerText = dataJson.winningAmount;
    $('#WinnerModal').modal('show');
    let walletElement = document.getElementById("Wallet");
    walletElement.innerText = `${dataJson.wallet} ₹`;
});



connection.on("UpdateTimer", (message) => {
    var timerElement = document.getElementById("timer");
    timerElement.innerHTML = `${message}`;
});
connection.on("DisableButtons", () => {
    disableButtons();
});
connection.on("EnableButtons", () => {
    enableButtons();
});

function ConfirmBet(event) {

    if (totalBetAmmount > 0) {

        bettingJson = JSON.stringify(batingInfoArray);
        if (batingInfoArray.length > 0) {
            const cancelButton = document.getElementById("Cancelclick");
            const clickedButton = event.target;
            clickedButton.disabled = true;
            cancelButton.disabled = true;

            connection.invoke("ConfirmBetting", bettingJson).then((x) => {

                if (x.status == true) {

                    let walletElement = document.getElementById("Wallet");
                    walletElement.innerText = `${x.wallet_balance} ₹`;
                    localStorage.setItem('walletBalance', x.wallet_balance);
                    ShowAlert("success", "Your bet SuccessFully saved")
                    isConfirm = true;
                    batingInfoArray = [];
                } else {
                    ShowAlert("failure", x.message);
                }


            }).catch(function (error) {
                console.error(error);
                isConfirm = false;
                ShowAlert("failure", "We are Facing Some Problem please try again after some time!")
            });
        }

    } else {
        ShowAlert("failure", "Please select a number");
    }

}

connection.start()
    .catch(err => console.error(err));



