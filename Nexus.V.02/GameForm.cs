using System.Drawing;
using System.Windows.Forms;

namespace NEXUS
{
    internal sealed class GameForm : Form
    {
        private readonly Color fondo = Color.FromArgb(12, 12, 16);
        private readonly Color panel = Color.FromArgb(25, 25, 32);
        private readonly Color rojo = Color.FromArgb(220, 35, 45);
        private readonly Color texto = Color.Gainsboro;
        private readonly string[,] mapa = new string[10,10]
        {
            { "E",".",".",".",".",".",".",".",".","." },
            { ".",".","*",".",".",".",".","+",".","." },
            { ".","#",".",".","*",".",".",".",".","." },
            { ".",".",".","+",".",".","#",".",".","." },
            { ".","*",".",".",".",".",".",".","+", "." },
            { ".",".","#",".",".","R",".",".",".","." },
            { ".","+",".",".","#",".",".","*",".","." },
            { ".",".",".","*",".",".",".",".","#","." },
            { ".","#",".",".","+",".","*",".",".","." },
            { ".",".",".",".",".",".",".",".","O","." }
        };
        private readonly Button[,] celdas = new Button[10,10];
        private string nombre = "";
        private int edad, energia = 100, conexion = 100, vida = 100;
        private int x, y, recursos, celulas, realidad;
        private bool registrado, nucleo;
        private Label lblNombre = null!, lblVida = null!, lblEnergia = null!, lblConexion = null!, lblEstado = null!, lblTitulo = null!;
        private Panel area = null!;
        private readonly Random azar = new Random();

        public GameForm()
        {
            Text = "NEXUS V2.0 - MATRIX SYSTEM";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1000, 700);
            Size = new Size(1180, 790);
            BackColor = fondo;
            ForeColor = texto;
            Font = new Font("Segoe UI", 10);
            ShowRegistration();
        }

        private Label LabelText(string value, int size = 10, bool bold = false)
            => new Label { Text = value, ForeColor = texto, BackColor = Color.Transparent, AutoSize = true, Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular) };

        private Button MakeButton(string caption, EventHandler action)
        {
            var b = new Button { Text = caption, Dock = DockStyle.Fill, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(45, 25, 31), ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold), Margin = new Padding(5), Cursor = Cursors.Hand };
            b.FlatAppearance.BorderColor = rojo; b.FlatAppearance.BorderSize = 1; b.Click += action; return b;
        }

        private void ShowRegistration()
        {
            Controls.Clear();
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(35), BackColor = fondo };
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 28)); root.RowStyles.Add(new RowStyle(SizeType.Percent, 57)); root.RowStyles.Add(new RowStyle(SizeType.Percent, 15));
            var title = LabelText("N E X U S\nREGISTRO DE EXPLORADOR", 25, true); title.ForeColor = rojo; title.TextAlign = ContentAlignment.MiddleCenter; title.Dock = DockStyle.Fill;
            root.Controls.Add(title,0,0);
            var form = new TableLayoutPanel { Anchor = AnchorStyles.None, AutoSize = true, ColumnCount = 2, RowCount = 4, Width = 520, BackColor = panel, Padding = new Padding(24) };
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,35)); form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,65));
            var txtNombre = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI",12) };
            var numEdad = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 1, Maximum = 120, Value = 18, Font = new Font("Segoe UI",12) };
            var numEnergia = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 1, Maximum = 100, Value = 100, Font = new Font("Segoe UI",12) };
            var numConexion = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 1, Maximum = 100, Value = 100, Font = new Font("Segoe UI",12) };
            AddFormRow(form,"NOMBRE",txtNombre,0); AddFormRow(form,"EDAD",numEdad,1); AddFormRow(form,"ENERGÍA INICIAL",numEnergia,2); AddFormRow(form,"CONEXIÓN",numConexion,3);
            root.Controls.Add(form,0,1);
            var start = MakeButton("INICIAR EXPLORACIÓN", (s,e) => {
                if (string.IsNullOrWhiteSpace(txtNombre.Text)) { MessageBox.Show("Ingresa el nombre del explorador."); return; }
                nombre = txtNombre.Text.Trim(); edad = (int)numEdad.Value; energia = (int)numEnergia.Value; conexion = (int)numConexion.Value; registrado = true; ShowDashboard();
            });
            root.Controls.Add(start,0,2); Controls.Add(root);
        }

        private void AddFormRow(TableLayoutPanel t, string caption, Control input, int row)
        {
            t.RowStyles.Add(new RowStyle(SizeType.Absolute,48));
            var l = LabelText(caption,10,true); l.Dock = DockStyle.Fill; l.TextAlign = ContentAlignment.MiddleLeft;
            t.Controls.Add(l,0,row); t.Controls.Add(input,1,row);
        }

        private void ShowDashboard()
        {
            if (!registrado) return;
            Controls.Clear();
            var shell = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, Padding = new Padding(12), BackColor = fondo };
            shell.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,220)); shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute,72)); shell.RowStyles.Add(new RowStyle(SizeType.Percent,100));
            var header = new Panel { Dock = DockStyle.Fill, BackColor = panel };
            lblTitulo = LabelText("NEXUS SYSTEM V2.0  //  ONLINE",18,true); lblTitulo.ForeColor = rojo; lblTitulo.Location = new Point(18,20); header.Controls.Add(lblTitulo);
            shell.Controls.Add(header,0,0); shell.SetColumnSpan(header,2);
            var nav = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = panel, ColumnCount = 1, RowCount = 8, Padding = new Padding(8) };
            string[] items = { "EXPLORAR", "ANALIZAR ANOMALÍA", "RECUPERAR ENERGÍA", "PERFIL", "INVENTARIO", "CAMBIAR REALIDAD", "VOLVER AL INICIO", "SALIR" };
            EventHandler[] handlers = { (s,e)=>ShowMap(), (s,e)=>Analyze(), (s,e)=>Recover(), (s,e)=>Profile(), (s,e)=>Inventory(), (s,e)=>ChangeDimension(), (s,e)=>ShowRegistration(), (s,e)=>Close() };
            for(int i=0;i<items.Length;i++){ nav.RowStyles.Add(new RowStyle(SizeType.Percent,12.5f)); nav.Controls.Add(MakeButton(items[i],handlers[i]),0,i); }
            shell.Controls.Add(nav,0,1);
            area = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(17,17,22), Padding = new Padding(18), AutoScroll = true };
            shell.Controls.Add(area,1,1); Controls.Add(shell); ShowMap();
        }

        private void SetArea(string title)
        {
            area.Controls.Clear();
            var heading = LabelText(title,19,true); heading.ForeColor = rojo; heading.Location = new Point(10,8); area.Controls.Add(heading);
            lblEstado = LabelText("",10); lblEstado.Location = new Point(10,42); area.Controls.Add(lblEstado);
            UpdateStats();
        }

        private void UpdateStats()
        {
            if(lblEstado != null) lblEstado.Text = $"EXPLORADOR: {nombre}   |   VIDA {vida}/100   |   ENERGÍA {energia}/100   |   CONEXIÓN {conexion}/100   |   RECURSOS {recursos}   |   CÉLULAS {celulas}";
        }

        private void ShowMap()
        {
            SetArea($"MAPA DIMENSIONAL  //  REALIDAD {(realidad==0 ? "102" : "999")}");
            var grid = new TableLayoutPanel { Location = new Point(10,78), Size = new Size(570,510), ColumnCount=10, RowCount=10, BackColor=panel, Padding=new Padding(4) };
            for(int i=0;i<10;i++){grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,10));grid.RowStyles.Add(new RowStyle(SizeType.Percent,10));}
            for(int row=0;row<10;row++) for(int col=0;col<10;col++)
            {
                int cx=col, cy=row; var b=new Button { Dock=DockStyle.Fill, Margin=new Padding(2), FlatStyle=FlatStyle.Flat, Font=new Font("Consolas",13,FontStyle.Bold), ForeColor=Color.White, BackColor=CellColor(mapa[row,col]) };
                b.FlatAppearance.BorderColor=Color.FromArgb(65,65,75); b.Text=(cx==x&&cy==y)?"C":mapa[row,col]; b.Click+=(s,e)=>MoveTo(cx,cy); celdas[row,col]=b; grid.Controls.Add(b,col,row);
            }
            area.Controls.Add(grid);
            var legend=LabelText("C = CADETE     E = EXTRACCIÓN     * = RECURSO     + = ANOMALÍA     # = PELIGRO     R = ZONA DE ANOMALÍA     O = NÚCLEO",9);
            legend.Location=new Point(10,600); legend.MaximumSize=new Size(760,0); area.Controls.Add(legend);
            var controls=new FlowLayoutPanel { Location=new Point(610,100), Size=new Size(210,250), FlowDirection=FlowDirection.TopDown };
            controls.Controls.Add(LabelText("CONTROLES DE MOVIMIENTO",11,true));
            controls.Controls.Add(MakeButton("↑  ARRIBA", (s,e)=>MoveTo(x,y-1)));
            controls.Controls.Add(MakeButton("←  IZQUIERDA", (s,e)=>MoveTo(x-1,y)));
            controls.Controls.Add(MakeButton("→  DERECHA", (s,e)=>MoveTo(x+1,y)));
            controls.Controls.Add(MakeButton("↓  ABAJO", (s,e)=>MoveTo(x,y+1)));
            controls.Controls.Add(MakeButton("ATACAR", (s,e)=>Attack()));
            area.Controls.Add(controls);
            KeyPreview=true;
        }

        private Color CellColor(string cell) => cell switch { "*" => Color.FromArgb(65,75,95), "+" => Color.DarkRed, "#" => Color.FromArgb(90,25,25), "O" => Color.DarkGoldenrod, "R" => Color.DarkCyan, "E" => Color.FromArgb(35,80,55), _ => Color.FromArgb(35,35,43) };

        private void MoveTo(int nx,int ny)
        {
            if(nx<0||nx>9||ny<0||ny>9){Notice("LÍMITE DEL MAPA","No puedes salir de la dimensión.");return;}
            if(energia<2){Notice("ENERGÍA INSUFICIENTE","Necesitas al menos 2 puntos de energía.");return;}
            energia-=2; conexion=Math.Max(0,conexion-1); x=nx;y=ny;
            string cell=mapa[y,x];
            if(cell=="*"){recursos++;mapa[y,x]=".";Notice("RECURSO OBTENIDO","Has recogido un recurso dimensional.");}
            else if(cell=="+"){energia=Math.Max(0,energia-5);Notice("ANOMALÍA","La anomalía consumió 5 puntos de energía.");}
            else if(cell=="#"){vida=Math.Max(0,vida-10);Notice("ZONA DE PELIGRO","Has recibido 10 puntos de daño.");}
            else if(cell=="O"){if(MessageBox.Show("Has encontrado el núcleo energético. ¿Deseas registrarlo?","NEXUS",MessageBoxButtons.YesNo,MessageBoxIcon.Question)==DialogResult.Yes){nucleo=true;mapa[y,x]="E";Notice("MISIÓN COMPLETADA","Núcleo registrado. Datos enviados a la nave.");}}
            if(vida<=0){Notice("MISIÓN FALLIDA","El explorador perdió toda su vida.");}
            ShowMap();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if(area!=null && registrado){switch(keyData){case Keys.W:case Keys.Up:MoveTo(x,y-1);return true;case Keys.S:case Keys.Down:MoveTo(x,y+1);return true;case Keys.A:case Keys.Left:MoveTo(x-1,y);return true;case Keys.D:case Keys.Right:MoveTo(x+1,y);return true;}}
            return base.ProcessCmdKey(ref msg,keyData);
        }

        private void Analyze()
        {
            if(energia<20){Notice("ENERGÍA INSUFICIENTE","Se necesitan 20 puntos para analizar.");return;}
            energia-=20;conexion=Math.Max(0,conexion-5);Notice("ANÁLISIS COMPLETADO","⚡ Anomalía dimensional detectada. Costo: 20 de energía.");UpdateStats();
        }
        private void Recover(){energia=Math.Min(100,energia+20);conexion=Math.Min(100,conexion+10);Notice("RECUPERACIÓN","Se recuperaron hasta 20 puntos de energía.");UpdateStats();}
        private void Attack(){if(vida<=0)return;Notice("COMBATE","Has atacado al bicho dimensional. La zona queda despejada.");}
        private void Profile(){SetArea("PERFIL DEL EXPLORADOR");AddInfo($"Nombre: {nombre}");AddInfo($"Edad: {edad}");AddInfo($"Vida: {vida}/100");AddInfo($"Energía: {energia}/100");AddInfo($"Conexión: {conexion}/100");}
        private void Inventory(){SetArea("INVENTARIO");AddInfo($"RECURSOS DIMENSIONALES: {recursos}");AddInfo($"CÉLULAS DE ENERGÍA: {celulas}");var use=MakeButton("USAR CÉLULA DE ENERGÍA", (s,e)=>{if(celulas>0){celulas--;energia=Math.Min(100,energia+30);UpdateStats();Notice("INVENTARIO","Se usó una célula: +30 energía.");}else Notice("INVENTARIO","No tienes células de energía.");});use.Location=new Point(10,100);use.Size=new Size(260,45);area.Controls.Add(use);}
        private void ChangeDimension(){realidad=1-realidad;x=0;y=0;Array.Clear(mapa,0,mapa.Length);string[,] layout={{"E",".",".",".",".",".",".",".",".","."},{".","*",".","+","*",".",".",".",".","."},{".","#",".",".",".",".","#",".",".","."},{".",".","+","*",".",".",".",".",".","."},{".",".",".",".","#",".",".","+","*","."},{".",".","#",".",".","R",".",".",".","."},{".","+","*",".",".",".","#",".",".","."},{".",".",".","*",".",".",".",".","#","."},{".","#",".",".","+","*",".",".",".","."},{".",".",".",".",".",".",".",".","O","."}};for(int r=0;r<10;r++)for(int c=0;c<10;c++)mapa[r,c]=layout[r,c];ShowMap();}
        private void AddInfo(string s){var l=LabelText(s,13);l.Location=new Point(18,75+area.Controls.Count*34);area.Controls.Add(l);}
        private void Notice(string title,string message)=>MessageBox.Show(message,title,MessageBoxButtons.OK,MessageBoxIcon.Information);
    }
}