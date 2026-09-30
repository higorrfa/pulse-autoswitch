# PULSE AutoSwitch

<img src="assets/pulse.png" alt="PULSE AutoSwitch" width="64" />

[English](#english) · [Português](#português)

## English

Automatic audio routing, battery status and a hardware volume overlay for the **Sony PULSE 3D on Windows**.

Turn the headset on to send audio to it. Turn it off, or unplug the receiver, to return to your selected monitor or speakers. The application reads the wireless connection state: USB presence alone cannot distinguish an active headset from a powered-off headset.

**Development preview.** Routing, the Desktop launcher and volume overlay have been confirmed on one Windows 11 PC. Media shortcuts and battery accuracy have limitations. This is an independent project, unaffiliated with Sony or PlayStation.

### Features

- Switch between the headset and a chosen fallback output automatically.
- Keep the output unchanged when toggling microphone mute.
- Minimize to the system tray; optionally start with Windows.
- Purple battery percentage icon and battery flyout.
- Hardware volume in the dashboard and a temporary overlay without taking focus.
- Desktop executable with an embedded application path.
- Experimental CHAT / GAME track shortcuts, limited by the hardware balance range.

### Requirements and installation

Use 64-bit Windows, .NET Framework 4.8 or later, and a PULSE 3D receiver **VID `054C`, PID `0D5E`, interface `MI_03`**. Installation scripts require PowerShell 5.1 or later. PULSE Elite and PlayStation Link are not supported.

1. Extract the entire preview ZIP to a permanent, writable folder.
2. Follow [Installation](docs/INSTALLATION.md) to associate **WinUSB with interface `054C / 0D5E / 03` only**, using the [official Zadig download](https://zadig.akeo.ie/). Leave the audio interface on its Windows audio driver.
3. Open PowerShell in the extracted `PulseAutoSwitch` folder:

```powershell
./scripts/CheckRequirements.ps1
./scripts/Install.ps1 -Force
```

4. Select **Headset output** and **Fallback output**, then enable **automatic routing**.
5. Test headset off / on while keeping the receiver connected; then test removing and reconnecting the receiver.

The installer checks prerequisites, creates **Start PULSE AutoSwitch.exe** on the Desktop and opens the application. Driver association requires administrator approval; normal use does not. External prerequisites and official download links are listed in [Dependencies](docs/DEPENDENCIES.md).

New installations have automation and experimental shortcuts disabled. `config.xml` is stored beside the application. Keep the installation folder in place; regenerate the launcher if you move it:

```powershell
./scripts/CreateDesktopLauncher.ps1 -ApplicationDirectory . -Force
```

**Minimize** and closing the window keep the application in the tray. **Quit** stops it. **Advanced** contains experimental shortcuts and diagnostics. **Start with Windows** controls a shortcut in your Startup folder, using `--tray`; validation at the next Windows sign-in is still pending.

### Battery, volume and limitations

The battery number is the receiver's reading, which can update in steps or remain at 100% after short usage. The flyout identifies the last reading when disconnected. Battery accuracy and charging indication are not calibrated.

The volume overlay shows the headset's own percentage for approximately two seconds. Windows and player volumes can differ. The overlay is rendered by this application.

CHAT / GAME shortcuts depend on balance changes and stop at the corresponding 0 / 100 limit. OFF / MONITOR controls sidetone; no usable event was observed for play / pause. Applications with a fixed output can ignore Windows default output changes. An unknown receiver state keeps the current output. The preview is unsigned; default output selection uses the undocumented `IPolicyConfig` interface.

### Build and package

From the source repository:

```powershell
./scripts/Build.ps1
./scripts/Test.ps1
./scripts/Install.ps1 -Force
./scripts/Package.ps1
```

The framework compiler builds to `bin/`; the preview ZIP is written to `dist/`. The ZIP includes binaries, installation scripts, launcher source, icons, documentation and the MIT license. It excludes local settings, logs and machine-specific driver packages. An MSBuild project is also available in `src/PulseAutoSwitch.csproj` for environments with .NET Framework 4.8 development components.

### Documentation

[Installation and removal](docs/INSTALLATION.md) · [Dependencies](docs/DEPENDENCIES.md) · [Protocol](docs/PROTOCOL.md) · [Validation](docs/VALIDATION.md) · [Publishing](docs/PUBLISHING.md) · [Contributing](CONTRIBUTING.md) · [Changelog](CHANGELOG.md) · [Third-party references](THIRD_PARTY_NOTICES.md)

## Português

Troca automática de áudio, bateria e indicador de volume para o **Sony PULSE 3D no Windows**.

Ligue o fone para enviar o áudio para ele. Desligue o fone ou retire o dongle para voltar ao monitor ou às caixas de som escolhidas. O aplicativo lê a conexão sem fio: a presença do dongle na USB, sozinha, não indica que o fone está ligado.

**Prévia em desenvolvimento.** A troca de áudio, o iniciador da Área de Trabalho e a barra de volume foram confirmados em um PC com Windows 11. Os atalhos de mídia e a precisão da bateria têm limitações. Projeto independente, sem vínculo com Sony ou PlayStation.

### Recursos

- Alternância automática entre o fone e uma saída de retorno configurável.
- O botão mute não altera a saída de áudio.
- Minimização para a bandeja e inicialização opcional com o Windows.
- Ícone roxo com porcentagem da bateria e painel de leitura.
- Volume do próprio fone na janela e em uma barra temporária que não tira o foco.
- Executável da Área de Trabalho com o caminho do aplicativo incorporado.
- Atalhos experimentais CHAT / GAME, limitados pelo balanço do próprio fone.

### Requisitos e instalação

Use Windows de 64 bits, .NET Framework 4.8 ou mais recente e o dongle PULSE 3D **VID `054C`, PID `0D5E`, interface `MI_03`**. Os scripts exigem PowerShell 5.1 ou mais recente. PULSE Elite e PlayStation Link não são suportados.

1. Extraia o ZIP completo para uma pasta permanente com permissão de gravação.
2. Siga [Installation](docs/INSTALLATION.md) para associar **WinUSB somente à interface `054C / 0D5E / 03`**, usando o [Zadig oficial](https://zadig.akeo.ie/). Mantenha a interface de áudio com o driver de áudio do Windows.
3. Abra o PowerShell na pasta extraída `PulseAutoSwitch`:

```powershell
./scripts/CheckRequirements.ps1
./scripts/Install.ps1 -Force
```

4. Escolha o fone em **Headset output** e o monitor ou caixas de som em **Fallback output**. Ative a troca automática.
5. Teste desligar e religar o fone mantendo o dongle conectado; depois teste retirar e reinserir o dongle.

O instalador verifica os requisitos, cria **Start PULSE AutoSwitch.exe** na Área de Trabalho e abre o aplicativo. A associação do driver exige administrador; o uso normal não exige. Consulte [Dependencies](docs/DEPENDENCIES.md) para os links oficiais e o conteúdo do pacote.

A automação e os atalhos experimentais começam desativados. As configurações ficam em `config.xml`, junto ao aplicativo. Se mover a pasta, recrie o iniciador:

```powershell
./scripts/CreateDesktopLauncher.ps1 -ApplicationDirectory . -Force
```

**Minimize** e o X da janela mantêm o aplicativo na bandeja. **Quit** encerra. **Advanced** mostra atalhos experimentais e diagnóstico. **Start with Windows** controla o atalho de inicialização com `--tray`; a confirmação no próximo login ainda está pendente. A interface do aplicativo está em inglês.

### Bateria, volume e limitações

A porcentagem da bateria é informada pelo dongle e pode mudar em etapas ou permanecer em 100% após pouco uso. Quando desconectado, o painel identifica a última leitura. A precisão e a indicação de carregamento não foram calibradas.

A barra de volume mostra o ajuste do próprio fone por cerca de dois segundos. O volume do Windows e o do player podem ser diferentes. Esse indicador é desenhado pelo aplicativo.

CHAT / GAME dependem da mudança do balanço e param nos limites correspondentes de 0 / 100. OFF / MONITOR controla o retorno da própria voz; não foi detectado um evento utilizável para play / pause. Aplicativos com saída fixa podem ignorar a troca do padrão do Windows. Estado desconhecido mantém a saída atual. A prévia não possui assinatura digital e usa `IPolicyConfig`, uma interface não documentada para escolher a saída padrão.

### Compilar e gerar pacote

Na raiz do código-fonte:

```powershell
./scripts/Build.ps1
./scripts/Test.ps1
./scripts/Install.ps1 -Force
./scripts/Package.ps1
```

Os executáveis são gerados em `bin/` e o ZIP em `dist/`. O pacote contém executáveis, scripts de instalação, fonte do iniciador, ícones, documentação e licença MIT. Configurações pessoais, logs e pacotes de driver gerados em um computador ficam fora do pacote e do Git.

## License / Licença

[MIT](LICENSE). Sony, PlayStation and PULSE names identify compatible hardware and do not imply endorsement. Os nomes identificam o hardware compatível e não indicam afiliação.
