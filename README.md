# dotfiles

[chezmoi](https://www.chezmoi.io/) ベースにしていちからやり直します。

## 自動インストールできないもの

- フォント
  - [HackGen](https://github.com/yuru7/HackGen)
- Windows Terminal の設定

### linuxbrew でインストールした zsh を有効にする

```shell
which zsh | sudo tee -a /etc/shells
chsh -s /home/linuxbrew/.linuxbrew/bin/zsh
```

## なんとかしたいやつ

- [Windows Terminal のデフォルト設定を指定したい](./private_dot_config/windows_terminal/settings.json)
