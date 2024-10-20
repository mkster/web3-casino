# Web3 Casino Game

This script implements a Web3-based multi token casino game in Unity using the Thirdweb SDK. It allows users to log in with their email, connect their wallets, and gamble using various cryptocurrencies on our Avalanche L1.

## Features:
- **Wallet Integration:** Users log in with an email to connect to their wallets via the Thirdweb In-App Wallet.
- **Casino Game:** Users can place bets, transferring tokens from their wallet to the casino's wallet. The outcome (win or loss) is determined randomly.
- **Token Transfers:** If the user wins, they receive double the tokens they gambled. If they lose, their tokens go to the casino.
- **Balance Updates:** Displays real-time user and casino wallet balances in both native currency and ERC-20 tokens.
- **Automatic Funding:** Ensures the user has a minimum balance by transferring tokens from the casino wallet.

## Dependencies:
- Thirdweb SDK
- Unity
- TMP (TextMeshPro)

## Usage:
1. **Login**: Enter your email to connect your wallet.
2. **Gamble**: Press the gamble button to play. Win or lose based on random chance.
3. **Balances**: Check your wallet balance in the interface.
