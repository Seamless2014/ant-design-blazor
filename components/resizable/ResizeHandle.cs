using System;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace AntDesign
{
    public class ResizeHandle : AntDomComponentBase, IDisposable
    {
        [Inject]
        private IJSRuntime JsRuntimeCurrent { get; set; }

        [Parameter]
        public BsSettings BsSettings { get; set; }

        [Parameter]
        public Action<bool, int, int> OnPositionChange { get; set; }

        [Parameter]
        public Action<int, int, int> OnDiagonalPositionChange { get; set; }

        [Parameter]
        public Action<int, int, int> OnDragStart { get; set; }

        [Parameter]
        public Action<int, int, int> OnDragEnd { get; set; }

        private BSplitter BSplitter { get; set; } = new BSplitter();

        private bool _dragMode = false;

        [Parameter]
        public bool EnableRender { get; set; } = true;

        protected override void OnInitialized()
        {
            BSplitter.BsbSettings = BsSettings;

            BSplitter._propertyChanged = BSplitter_PropertyChanged;

            _dragMode = false;

            base.OnInitialized();
        }

        protected override bool ShouldRender()
        {
            return EnableRender;
        }

        private void BSplitter_PropertyChanged()
        {
            EnableRender = true;
            StateHasChanged();
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (EnableRender)
            {
                base.BuildRenderTree(builder);

                int k = 0;
                builder.OpenElement(k++, "div");
                builder.AddAttribute(k++, "id", BSplitter.BsbSettings.ID);
                builder.AddAttribute(k++, "style", BSplitter.BsbSettings.GetStyle());
                //builder.AddAttribute(k++, "ref", Ref);
                builder.AddAttribute(k++, "onpointerdown", EventCallback.Factory.Create<PointerEventArgs>(this, OnPointerDown));
                builder.AddAttribute(k++, "onpointermove", EventCallback.Factory.Create<PointerEventArgs>(this, OnPointerMove));
                builder.AddAttribute(k++, "onpointerup", EventCallback.Factory.Create<PointerEventArgs>(this, OnPointerUp));

                builder.AddEventPreventDefaultAttribute(k++, "onmousemove", true);
                //builder.AddAttribute(k++, "onmousemove", EventCallback.Factory.Create<MouseEventArgs>(this, "return false;")); //event.preventDefault()

                builder.CloseElement();

                EnableRender = false;
            }
        }

        private void OnPointerDown(PointerEventArgs e)
        {
            JsRuntimeCurrent.InvokeAsync<bool>(JSInteropConstants.SetPointerCapture, BSplitter.BsbSettings.ID, e.PointerId);
            _dragMode = true;

            if (BsSettings.IsDiagonal)
            {
                BSplitter.PreviousPosition = (int)e.ClientX;
                BSplitter.PreviousPosition2 = (int)e.ClientY;
            }
            else
            {
                if (BsSettings.VerticalOrHorizontal)
                {
                    BSplitter.PreviousPosition = (int)e.ClientY;
                    BSplitter.PreviousPosition2 = (int)e.ClientX;
                }
                else
                {
                    BSplitter.PreviousPosition = (int)e.ClientX;
                    BSplitter.PreviousPosition2 = (int)e.ClientY;
                }
            }

            OnDragStart?.Invoke(BSplitter.BsbSettings.Index, (int)e.ClientX, (int)e.ClientY);
        }

        private void OnPointerMove(PointerEventArgs e)
        {
            if (_dragMode)
            {
                if (e.Buttons == 1)
                {
                    int newPosition = 0;
                    int newPosition2 = 0;

                    if (BsSettings.IsDiagonal)
                    {
                        newPosition = (int)e.ClientX;
                        newPosition2 = (int)e.ClientY;

                        if (BSplitter.PreviousPosition != newPosition || BSplitter.PreviousPosition2 != newPosition2)
                        {
                            OnDiagonalPositionChange?.Invoke(BsSettings.Index, newPosition - BSplitter.PreviousPosition, newPosition2 - BSplitter.PreviousPosition2);

                            BSplitter.PreviousPosition = newPosition;
                            BSplitter.PreviousPosition2 = newPosition2;
                        }
                    }
                    else
                    {
                        if (BsSettings.VerticalOrHorizontal)
                        {
                            newPosition = (int)e.ClientY;
                            newPosition2 = (int)e.ClientX;
                        }
                        else
                        {
                            newPosition = (int)e.ClientX;
                            newPosition2 = (int)e.ClientY;
                        }

                        if (Math.Abs(BSplitter.PreviousPosition2 - newPosition2) < 100)
                        {
                            if (BSplitter.PreviousPosition != newPosition)
                            {
                                OnPositionChange?.Invoke(BsSettings.VerticalOrHorizontal, BsSettings.Index, newPosition - BSplitter.PreviousPosition);

                                BSplitter.PreviousPosition = newPosition;
                            }
                        }
                        //else
                        //{
                        //    //BsJsInterop.StopDrag(bSplitter.bsbSettings.ID);
                        //}
                    }
                }
            }
        }

        private void OnPointerUp(PointerEventArgs e)
        {
            JsRuntimeCurrent.InvokeAsync<bool>(JSInteropConstants.ReleasePointerCapture, BSplitter.BsbSettings.ID, e.PointerId);
            _dragMode = false;

            OnDragEnd?.Invoke(BSplitter.BsbSettings.Index, (int)e.ClientX, (int)e.ClientY);
        }

        public void Dispose()
        {
        }

        public void SetColor(string c)
        {
            BSplitter.BsbSettings.BgColor = c;
            EnableRender = true;
            StateHasChanged();
        }
    }
}
