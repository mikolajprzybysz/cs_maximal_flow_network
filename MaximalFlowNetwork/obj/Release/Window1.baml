   M S B A M L   `   `   `  ���� O  KWPFExtensions, Version=1.0.3437.34043, Culture=neutral, PublicKeyToken=nullV;clr-namespace:WPFExtensions.Controls;assembly=WPFExtensionsWPFExtensions.Controls  U QGraphSharp.Controls, Version=1.0.3438.42202, Culture=neutral, PublicKeyToken=nullV>clr-namespace:GraphSharp.Controls;assembly=GraphSharp.ControlsGraphSharp.Controls M IMaximalFlowNetwork, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null7 clr-namespace:MaximalFlowNetworkMaximalFlowNetwork     MaximalFlowNetwork.Window1   \ XPresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    8x,http://schemas.microsoft.com/winfx/2006/xaml    Jzoom;clr-namespace:WPFExtensions.Controls;assembly=WPFExtensions    S
graphsharp>clr-namespace:GraphSharp.Controls;assembly=GraphSharp.Controls    /myNS clr-namespace:MaximalFlowNetwork    -     �Name$
  root�� � Title$ Maximal Flow Network��$	��613��$	��800��.���  � RowDefinitions �� $��32 ��� $
��518* ��� $	��25* � � ColumnDefinitions �� $��120.014 ��� $��657.986* �.+��� -   $��stackPanel1��
����İ�$��3��$��2��$
��Left=�$	��136��.+��� -   $��button1��$��35��$	��100��$��2,12,2,2q�.��New�� -   $��button2��$��35��$	��100��$��2,2,2,2q�.��Load�� -   $��button3��$��35��$	��100��$��2,2,2,2q�.��Save�� -   $��
separator1��$��5.001��$��120.024���� -   $��
wrapPanel1��.+��� -   $��label1��$��28.179��$
��Auto��$��Center=�$	��Top=�$��Center=�$
��Left=�.��Mode of calculation:	� -	   $��radioButton1��$��16.362��$��Center=�$��4,0,0,4q�$
��Left=�$	��100��.��Step by step�� -
   $��button9��$��35��$	��100��$y�	Collapsed=�$��Center=�$��4q�.��Perform Step	� -   $��radioButton2��$��16.362��$��Center=�$��4,10,0,4q� d� 	IsChecked$
 TrueY�$
��Left=�$	��100��.��Only result�� -   $��button4��$��35��$	��100��$��Center=�$��4q�.��Start Calculation�� -   $��button11��$��35��$	��100��$y�Hidden=�$��Center=�$��4q�.��	Restart�� -   $��FlowInfo_stackPanel��$��69��$
��Auto��$y�Hidden=�.+��� -   $��label5��$��38��$	��125��$��Bottom=�$��Center=�.��Value of flow to add:�� -   $��flowInfoLabel��$��20(�$��38��$��Center=�$	��100��� -   $��
wrapPanel2��
����İ�$��14,0,0,0q�$��1��.+��� -   $��button7��
�������$��28��$��20,2,2,2q�$��28��.���� $��Right=�$��	arrow.bmp���� -   $��button5��$��28��$��28��$��2,2,2,2q�
�������$��Right=�.���� $��node.bmp��$��Right=��� -   $��button6��$��28��$��28��$��2,2,2,2q�
�������$��Right=�.���� $��edge.bmp��$��Right=��� -   $��button8��$��28��$��28��$��2,2,2,2q�
�������$��Right=�.���� $��	tools.bmp��$��Right=��� -   $��button10��
�������$��28��$��2,2,2,2q�$��28��$��Right=�.���� $��del.bmp��$��Right=��� -   $��label2��$��28��$��50��$��20,2,2,2q�$��Center=�$��Right=�.��Width:�� -   $��textBoxWidth��$��23��$��50��$��2q�$��Center=�.��500�� -   $��label3��$��28��$��50��$��20,2,2,2q�$��Right=�$��Center=�.��	Height:�� -   $��textBoxHeight��$��23��$��50��$��2q�$��Center=�.��500� -   $��
graphPanel��$��1��$��1��$��2��$��14,0,0,0q�.+�� -   $��
wrapPanel3��
����İ�$��25��$��-4,0,0,0q�$��Bottom=�.+��� -   $��TipLabel��$��28��$��661.986��]� -   $��
dockPanel1��$��0,0,0,25q�.+��� 
�������$��2q�._�(   "WPFExtensions.Controls.ZoomControl     Zoom$ 1Q�$
��Auto��$
��Auto��
���   �.��#  MaximalFlowNetwork.GraphCanva  -     Name$ 
graphCanva��$��1q�$	��500��$	��500��$	��Top=�