const resultElement = document.getElementById("result");
const walletBalanceElement = document.getElementById("Wallet");
const headButton = document.getElementById("HeadsBtn");
const tailButton = document.getElementById("TailsBtn");
const confirmButton = document.getElementById("Confirmclick");
const clearButton = document.getElementById("Cancelclick");
const betelement = document.getElementById("TotalBet");
const storedWalletBalance = localStorage.getItem('walletBalance');
const priceButtons = document.getElementById('pricebutons');
let coin = document.querySelector(".coin");
let betOnHeads = 0;
let betOnTails = 0;
let isConfirm = false;

const coinData = { "Heads": 0, "Tails": 0 };

headButton.addEventListener('click', () => installBet('Heads'));
tailButton.addEventListener('click', () => installBet('Tails'));

clearButton.addEventListener("click", () => {
    if (!isConfirm) {
        const basgeButton = document.querySelectorAll("#ButonsCoins button");
        basgeButton.forEach((element) => {
            let childelement = element.querySelector("span");
            if (childelement) {
                element.removeChild(childelement);
            }
        });
        betOnHeads = 0;
        betOnTails = 0;
        betelement.innerText = (betOnHeads + betOnTails);
    }
});
confirmButton.addEventListener("click", () => {
    if (!isConfirm) {
        if ((coinData.Heads + coinData.Tails) > 0) {
            var betJson = JSON.stringify(coinData);
            connection.invoke("ConfirmCoinBet", betJson).then((x) => {
                if (x.status == true) {
                    let walletElement = document.getElementById("Wallet");
                    walletElement.innerText = `${x.wallet_balance} ₹`;
                    ShowAlert("success", "Your bet SuccessFully saved")
                    isConfirm = true;
                } else {
                    ShowAlert("failure", x.message);
                }

            }).catch(function (error) {
                console.error(error);
                ShowAlert("failure", "We are Facing Some Problem please try again after some time!")
            });
        }
    }
});


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

function installBet(coinSide) {
    const selectedAmountInput = priceButtons.querySelector('input[type="radio"]:checked');
    const selectedAmount = selectedAmountInput.checked ? parseInt(selectedAmountInput.value) : 0;

    if (!coinData[coinSide]) {
        coinData[coinSide] = selectedAmount;
    } else {
        coinData[coinSide] += selectedAmount;
    }


    if (storedWalletBalance !== null) {
        const walletBalance = parseInt(storedWalletBalance);


        if (coinSide === "Heads") {
            betOnHeads += selectedAmount;

            if (walletBalance >= (betOnHeads + betOnTails)) {
                const badge = document.querySelector('#HeadsBtn span');
                if (!badge) {
                    addBadgeToButton(headButton, betOnHeads);

                } else {
                    badge.innerHTML = `${betOnHeads}₹`
                }
            } else {
                ShowAlert("failure", "Your bet reached to your wallet balance please recharge your wallet to continoue!!");
                betOnHeads -= selectedAmount;
            }

        }
        else {

            const walletBalance = parseInt(storedWalletBalance);
            betOnTails += selectedAmount;
            if (walletBalance >= (betOnHeads + betOnTails)) {
                //betelement.innerText = (betOnHeads + betOnTails);

                const badge = document.querySelector('#TailsBtn span');
                if (!badge) {
                    addBadgeToButton(tailButton, betOnTails);

                } else {
                    badge.innerHTML = `${betOnTails}₹`
                }
            } else {
                ShowAlert("failure", "Your bet reached to your wallet balance please recharge your wallet to continoue!!");
                betOnTails -= selectedAmount;
            }

        }

        betelement.innerText = betOnHeads + betOnTails;
        console.log(`Head : ${betOnHeads}`);
        console.log(`Tail : ${betOnTails}`);
    }


}
function disableButtons() {
    document.querySelectorAll("button").forEach((button) => {
        button.disabled = true;
    });
    checkboxes.forEach((check) => { check.disabled = true })
}

// Function to enable all buttons
function enableButtons() {
    document.querySelectorAll("button").forEach((button) => {
        button.disabled = false;
    });
    checkboxes.forEach((x) => x.disabled = false);
}
function addBadgeToButton(targetButton, ammount) {

    const badgeContainer = document.createElement('span');
    badgeContainer.classList.add('position-absolute', 'top-0', 'start-50', 'translate-middle', 'badge', 'rounded-pill', 'bg-success');
    badgeContainer.innerHTML = `${ammount}₹`;
    targetButton.appendChild(badgeContainer);
}

function startSpinning() {

    coin.innerHTML = '<div class="heads"><img src="/heads.png"></div><div class="tails"><img src="/tails.png"></div>';
    coin.classList.add("coin-spin");
}
function showResult(result) {
    coin.classList.remove("coin-spin");
    resultElement.innerText = result;
    if (result === "Heads") {        
        coin.innerHTML = '<div><img src="/heads.png"></div>';
    } else {
        coin.innerHTML = '<div><img src="/tails.png"></div>';
    }
}

// Connect to the SignalR hub
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/updateHub")
    .build();

connection.on("UpdateTimer", (message) => {
    var timerElement = document.getElementById("timer");
    timerElement.innerHTML = `${message}`;
});
connection.on("DisableButtons", () => {
    startSpinning();
    disableButtons();
});
connection.on("EnableButtons", () => {
    enableButtons();
});

connection.on("updateCoinResult", (result) => {
    if (result != null) {
        showResult(result);
    }
});

connection.start()
    .catch(err => console.error(err));
